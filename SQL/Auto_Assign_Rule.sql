USE [tool_eduipment_system]
GO

/* =====================================================================
   Auto Assign Rule : เลือก Status Use / ผู้รับผิดชอบ อัตโนมัติ
   จัดการจากหน้า Equipment_Setting.aspx (เมนู System Setup)
   ---------------------------------------------------------------------
   กติกา Match
     - เงื่อนไขชนิดเดียวกันหลายค่า = OR   (Dept 5510 หรือ 5515)
     - Dept กับ Location (Cell/Station)  = AND
     - Cell กับ Station ในกฎเดียวกัน      = ตาม loc_logic ('OR' ค่าเริ่มต้น / 'AND')
     - ตรงหลายกฎ → กฎที่เจาะจงกว่าชนะ (Dept+Location > Location > Dept)
                   เสมอ → priority มากกว่าชนะ → rule_id น้อยกว่าชนะ
   ค่าเงื่อนไขเก็บเป็น ID : DEPT = dept_code , CELL = location2_id , STATION = location3_id
   ===================================================================== */

/* ---------- 1) กฎ ---------- */
IF OBJECT_ID('dbo.Auto_Assign_Rule', 'U') IS NULL
CREATE TABLE [dbo].[Auto_Assign_Rule](
    [rule_id]        [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [rule_name]      [nvarchar](100) NOT NULL,
    [status_use_id]  [int] NULL,                                   -- NULL = ไม่กำหนด Status
    [loc_logic]      [nvarchar](3) NOT NULL DEFAULT ('OR'),        -- Cell กับ Station : OR / AND
    [priority]       [int] NOT NULL DEFAULT (0),
    [is_active]      [bit] NOT NULL DEFAULT (1),
    [remark]         [nvarchar](250) NULL,
    [create_by]      [nvarchar](50) NULL,
    [create_date]    [datetime] NOT NULL DEFAULT (GETDATE()),
    [update_by]      [nvarchar](50) NULL,
    [update_date]    [datetime] NULL,
    CONSTRAINT [CK_Auto_Assign_Rule_loc_logic] CHECK ([loc_logic] IN ('OR', 'AND'))
)
GO

/* ---------- 2) เงื่อนไข ---------- */
IF OBJECT_ID('dbo.Auto_Assign_Rule_Cond', 'U') IS NULL
CREATE TABLE [dbo].[Auto_Assign_Rule_Cond](
    [cond_id]     [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [rule_id]     [int] NOT NULL,
    [cond_type]   [nvarchar](10) NOT NULL,    -- DEPT / CELL / STATION
    [cond_value]  [nvarchar](50) NOT NULL,    -- ID
    CONSTRAINT [FK_Auto_Assign_Rule_Cond_Rule] FOREIGN KEY ([rule_id])
        REFERENCES [dbo].[Auto_Assign_Rule]([rule_id]) ON DELETE CASCADE,
    CONSTRAINT [CK_Auto_Assign_Rule_Cond_type] CHECK ([cond_type] IN ('DEPT', 'CELL', 'STATION')),
    CONSTRAINT [UQ_Auto_Assign_Rule_Cond] UNIQUE ([rule_id], [cond_type], [cond_value])
)
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Auto_Assign_Rule_Cond_type_value')
    CREATE INDEX [IX_Auto_Assign_Rule_Cond_type_value]
        ON [dbo].[Auto_Assign_Rule_Cond]([cond_type], [cond_value]) INCLUDE ([rule_id]);
GO

/* ---------- 3) ผู้รับผิดชอบของกฎ (seq 1 = เลือกอัตโนมัติ) ---------- */
IF OBJECT_ID('dbo.Auto_Assign_Rule_Resp', 'U') IS NULL
CREATE TABLE [dbo].[Auto_Assign_Rule_Resp](
    [rule_id]  [int] NOT NULL,
    [emp_id]   [nvarchar](50) NOT NULL,
    [seq]      [int] NOT NULL,
    CONSTRAINT [PK_Auto_Assign_Rule_Resp] PRIMARY KEY ([rule_id], [emp_id]),
    CONSTRAINT [FK_Auto_Assign_Rule_Resp_Rule] FOREIGN KEY ([rule_id])
        REFERENCES [dbo].[Auto_Assign_Rule]([rule_id]) ON DELETE CASCADE
)
GO

/* ---------- 4) ประวัติการแก้ไข ---------- */
IF OBJECT_ID('dbo.Auto_Assign_Rule_History', 'U') IS NULL
CREATE TABLE [dbo].[Auto_Assign_Rule_History](
    [hist_id]      [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [rule_id]      [int] NOT NULL,                -- ไม่ผูก FK เพื่อเก็บประวัติแม้ลบกฎแล้ว
    [action]       [nvarchar](20) NOT NULL,       -- CREATE / UPDATE / ACTIVE / INACTIVE / DELETE
    [detail]       [nvarchar](max) NULL,          -- สรุปกฎ (ก่อน → หลัง)
    [action_by]    [nvarchar](50) NULL,
    [action_date]  [datetime] NOT NULL DEFAULT (GETDATE())
)
GO

/* =====================================================================
   5) sp_AutoAssign_Resolve : หากฎที่ตรงที่สุด 1 กฎ
      คืนค่า 1 แถวต่อผู้รับผิดชอบ (เรียงตาม seq) — ไม่มีผู้รับผิดชอบ = 1 แถว emp_id NULL
      ไม่ตรงกฎไหน = ไม่มีแถว (ให้หน้าเว็บใช้ค่าเดิม : Responsible_Dept / 5505 → Spare)
   ===================================================================== */
IF OBJECT_ID('dbo.sp_AutoAssign_Resolve', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_AutoAssign_Resolve];
GO
CREATE PROCEDURE [dbo].[sp_AutoAssign_Resolve]
    @dept        nvarchar(50),
    @cell_id     nvarchar(50) = NULL,
    @station_id  nvarchar(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @dept       = NULLIF(LTRIM(RTRIM(@dept)), '');
    SET @cell_id    = NULLIF(LTRIM(RTRIM(@cell_id)), '');
    SET @station_id = NULLIF(LTRIM(RTRIM(@station_id)), '');

    ;WITH R AS (
        SELECT  r.rule_id, r.rule_name, r.status_use_id, r.loc_logic, r.priority,
                SUM(CASE WHEN c.cond_type = 'DEPT'    THEN 1 ELSE 0 END) AS nDept,
                SUM(CASE WHEN c.cond_type = 'DEPT'    AND c.cond_value = @dept       THEN 1 ELSE 0 END) AS mDept,
                SUM(CASE WHEN c.cond_type = 'CELL'    THEN 1 ELSE 0 END) AS nCell,
                SUM(CASE WHEN c.cond_type = 'CELL'    AND c.cond_value = @cell_id    THEN 1 ELSE 0 END) AS mCell,
                SUM(CASE WHEN c.cond_type = 'STATION' THEN 1 ELSE 0 END) AS nSta,
                SUM(CASE WHEN c.cond_type = 'STATION' AND c.cond_value = @station_id THEN 1 ELSE 0 END) AS mSta
        FROM    dbo.Auto_Assign_Rule r
        JOIN    dbo.Auto_Assign_Rule_Cond c ON c.rule_id = r.rule_id
        WHERE   r.is_active = 1
        GROUP BY r.rule_id, r.rule_name, r.status_use_id, r.loc_logic, r.priority
    ),
    M AS (
        SELECT  R.*,
                (CASE WHEN nDept > 0 THEN 1 ELSE 0 END) +
                (CASE WHEN nCell + nSta > 0 THEN 2 ELSE 0 END) AS score
        FROM    R
        WHERE   (nDept = 0 OR mDept > 0)
          AND   (     (nCell = 0 AND nSta = 0)
                  OR  (loc_logic = 'OR'  AND (mCell > 0 OR mSta > 0))
                  OR  (loc_logic = 'AND' AND (nCell = 0 OR mCell > 0) AND (nSta = 0 OR mSta > 0)) )
    ),
    BEST AS (
        SELECT TOP 1 * FROM M ORDER BY score DESC, priority DESC, rule_id ASC
    )
    SELECT  B.rule_id, B.rule_name, B.status_use_id, S.status_name, B.score,
            RR.emp_id, RR.seq, PIC.pic_dept
    FROM    BEST B
    LEFT JOIN dbo.tool_equipment_status S ON S.status_id = B.status_use_id
    LEFT JOIN dbo.Auto_Assign_Rule_Resp RR ON RR.rule_id = B.rule_id
    OUTER APPLY (SELECT TOP 1 RD.pic_dept FROM dbo.Responsible_Dept RD
                 WHERE LTRIM(RTRIM(RD.emp_id)) = RR.emp_id) PIC
    ORDER BY RR.seq;
END
GO

/* =====================================================================
   ตัวอย่างกฎ (ตามที่ตกลง) — แก้ ID Cell/Station ให้ตรง master ก่อนรัน
   *** ไม่รันอัตโนมัติ : เปิด comment แล้วใส่ ID จริง หรือสร้างจากหน้า Equipment_Setting ***
   ---------------------------------------------------------------------
   1) Dept 5510, 5515                              → 001566, 001234
   2) Dept 5510, 5515 & (Cell QC OR Station QC)    → 001566
   3) Dept 5520                                     → 000174
   4) Dept 5505 & Cell (Storage OR Control Room)   → 000172
   5) Dept 5505 & Cell PDE (M)                     → 002869
   ===================================================================== */
