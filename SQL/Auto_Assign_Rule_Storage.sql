USE [tool_eduipment_system]
GO

/* =====================================================================
   Auto Assign : เพิ่มกฎสำหรับ Storage (storage.aspx — Modal AAD Confirm)
   ---------------------------------------------------------------------
   รันต่อจาก SQL/Auto_Assign_Rule.sql (รันซ้ำได้ — ทุกขั้นเช็คก่อนทำ)
   ** ถ้าเคยรันไฟล์นี้แล้ว (ก่อนแก้ 2.1) → รันซ้ำอีกครั้งเพื่อขยาย cond_type **

   1) Auto_Assign_Rule.apply_to      : WITHDRAW (ค่าเริ่มต้น / กฎเดิมทั้งหมด) / STORAGE
   2) Auto_Assign_Rule_Cond.cond_type : เพิ่ม STATUS_ITEM / OKNG / REPAIR / REASON
        STATUS_ITEM = 'Spare' / 'Not Use'           (Storage_List.Status_Item)
        OKNG        = 'OK' / 'NG'                   (Storage_List.OK_NG)
        REPAIR      = 'ซ่อม' / 'ไม่ซ่อม/ซ่อมไม่ได้'   (Storage_List.Repair_Status)
        REASON      = reason_ID                     (Reason_Borrow_Master , storageFlow = 'true')
      + ใช้ CELL / STATION (ที่จัดเก็บ) ได้เหมือนเดิม
   3) sp_AutoAssign_Resolve          : อ่านเฉพาะกฎ WITHDRAW (กันกฎ Storage ไปตรงกับ withdraw2)
   4) sp_AutoAssign_Resolve_Storage  : หากฎ Storage ที่ตรง 1 กฎ
        กติกา : ค่าในชนิดเดียวกัน = OR · ต่างชนิด = AND · Cell/Station ตาม loc_logic
        ตรงหลายกฎ → Priority มากกว่าชนะ → เงื่อนไขหลายชนิดกว่า (เจาะจงกว่า) ชนะ → rule_id น้อยกว่าชนะ
   5) Storage_List : Old/New Status + Old/New Responsible (เหมือน Withdraw_List)
   ===================================================================== */

/* ---------- 1) apply_to ---------- */
IF COL_LENGTH('dbo.Auto_Assign_Rule', 'apply_to') IS NULL
BEGIN
    ALTER TABLE [dbo].[Auto_Assign_Rule]
        ADD [apply_to] [nvarchar](10) NOT NULL
            CONSTRAINT [DF_Auto_Assign_Rule_apply_to] DEFAULT ('WITHDRAW') WITH VALUES;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Auto_Assign_Rule_apply_to')
    ALTER TABLE [dbo].[Auto_Assign_Rule]
        ADD CONSTRAINT [CK_Auto_Assign_Rule_apply_to] CHECK ([apply_to] IN ('WITHDRAW', 'STORAGE'));
GO

/* ---------- 2) cond_type ใหม่ ---------- */
/* 2.1) ขยาย cond_type : เดิม nvarchar(10) — 'STATUS_ITEM' ยาว 11 ตัว
        (Error : String or binary data would be truncated ... column 'cond_type')
        ต้องปลด UNIQUE / INDEX / CHECK ที่ผูกคอลัมน์ก่อน ALTER แล้วสร้างกลับ */
IF COL_LENGTH('dbo.Auto_Assign_Rule_Cond', 'cond_type') < 40          -- bytes : nvarchar(10) = 20 , nvarchar(20) = 40
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Auto_Assign_Rule_Cond')
        ALTER TABLE [dbo].[Auto_Assign_Rule_Cond] DROP CONSTRAINT [UQ_Auto_Assign_Rule_Cond];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Auto_Assign_Rule_Cond_type_value')
        DROP INDEX [IX_Auto_Assign_Rule_Cond_type_value] ON [dbo].[Auto_Assign_Rule_Cond];
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Auto_Assign_Rule_Cond_type')
        ALTER TABLE [dbo].[Auto_Assign_Rule_Cond] DROP CONSTRAINT [CK_Auto_Assign_Rule_Cond_type];

    ALTER TABLE [dbo].[Auto_Assign_Rule_Cond] ALTER COLUMN [cond_type] [nvarchar](20) NOT NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Auto_Assign_Rule_Cond')
    ALTER TABLE [dbo].[Auto_Assign_Rule_Cond]
        ADD CONSTRAINT [UQ_Auto_Assign_Rule_Cond] UNIQUE ([rule_id], [cond_type], [cond_value]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Auto_Assign_Rule_Cond_type_value')
    CREATE INDEX [IX_Auto_Assign_Rule_Cond_type_value]
        ON [dbo].[Auto_Assign_Rule_Cond]([cond_type], [cond_value]) INCLUDE ([rule_id]);
GO

/* 2.2) CHECK : ชนิดเงื่อนไขทั้งหมด */
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Auto_Assign_Rule_Cond_type')
    ALTER TABLE [dbo].[Auto_Assign_Rule_Cond] DROP CONSTRAINT [CK_Auto_Assign_Rule_Cond_type];
GO
ALTER TABLE [dbo].[Auto_Assign_Rule_Cond]
    ADD CONSTRAINT [CK_Auto_Assign_Rule_Cond_type]
        CHECK ([cond_type] IN ('DEPT', 'CELL', 'STATION', 'STATUS_ITEM', 'OKNG', 'REPAIR', 'REASON'));
GO

/* ---------- 3) sp_AutoAssign_Resolve : เฉพาะกฎ WITHDRAW (ที่เหลือเหมือนเดิมทุกบรรทัด) ---------- */
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
          AND   r.apply_to = 'WITHDRAW'                                -- ✅ ไม่อ่านกฎ Storage
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

/* ---------- 4) sp_AutoAssign_Resolve_Storage ---------- */
IF OBJECT_ID('dbo.sp_AutoAssign_Resolve_Storage', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_AutoAssign_Resolve_Storage];
GO
CREATE PROCEDURE [dbo].[sp_AutoAssign_Resolve_Storage]
    @status_item  nvarchar(50) = NULL,     -- Spare / Not Use
    @okng         nvarchar(50) = NULL,     -- OK / NG
    @repair       nvarchar(50) = NULL,     -- ซ่อม / ไม่ซ่อม/ซ่อมไม่ได้
    @reason       nvarchar(50) = NULL,     -- reason_ID
    @cell_id      nvarchar(50) = NULL,     -- Storage : Cell (location2_id)
    @station_id   nvarchar(50) = NULL      -- Storage : Station (location3_id)
AS
BEGIN
    SET NOCOUNT ON;

    SET @status_item = NULLIF(LTRIM(RTRIM(@status_item)), '');
    SET @okng        = NULLIF(LTRIM(RTRIM(@okng)), '');
    SET @repair      = NULLIF(NULLIF(LTRIM(RTRIM(@repair)), ''), '-');
    SET @reason      = NULLIF(LTRIM(RTRIM(@reason)), '');
    SET @cell_id     = NULLIF(LTRIM(RTRIM(@cell_id)), '');
    SET @station_id  = NULLIF(LTRIM(RTRIM(@station_id)), '');

    ;WITH R AS (
        SELECT  r.rule_id, r.rule_name, r.status_use_id, r.loc_logic, r.priority,
                SUM(CASE WHEN c.cond_type = 'STATUS_ITEM' THEN 1 ELSE 0 END) AS nSt,
                SUM(CASE WHEN c.cond_type = 'STATUS_ITEM' AND c.cond_value = @status_item THEN 1 ELSE 0 END) AS mSt,
                SUM(CASE WHEN c.cond_type = 'OKNG'        THEN 1 ELSE 0 END) AS nOk,
                SUM(CASE WHEN c.cond_type = 'OKNG'        AND c.cond_value = @okng        THEN 1 ELSE 0 END) AS mOk,
                SUM(CASE WHEN c.cond_type = 'REPAIR'      THEN 1 ELSE 0 END) AS nRp,
                SUM(CASE WHEN c.cond_type = 'REPAIR'      AND c.cond_value = @repair      THEN 1 ELSE 0 END) AS mRp,
                SUM(CASE WHEN c.cond_type = 'REASON'      THEN 1 ELSE 0 END) AS nRs,
                SUM(CASE WHEN c.cond_type = 'REASON'      AND c.cond_value = @reason      THEN 1 ELSE 0 END) AS mRs,
                SUM(CASE WHEN c.cond_type = 'CELL'        THEN 1 ELSE 0 END) AS nCell,
                SUM(CASE WHEN c.cond_type = 'CELL'        AND c.cond_value = @cell_id     THEN 1 ELSE 0 END) AS mCell,
                SUM(CASE WHEN c.cond_type = 'STATION'     THEN 1 ELSE 0 END) AS nSta,
                SUM(CASE WHEN c.cond_type = 'STATION'     AND c.cond_value = @station_id  THEN 1 ELSE 0 END) AS mSta
        FROM    dbo.Auto_Assign_Rule r
        JOIN    dbo.Auto_Assign_Rule_Cond c ON c.rule_id = r.rule_id
        WHERE   r.is_active = 1
          AND   r.apply_to = 'STORAGE'
        GROUP BY r.rule_id, r.rule_name, r.status_use_id, r.loc_logic, r.priority
    ),
    M AS (
        SELECT  R.*,
                (CASE WHEN nSt > 0 THEN 1 ELSE 0 END) + (CASE WHEN nOk > 0 THEN 1 ELSE 0 END) +
                (CASE WHEN nRp > 0 THEN 1 ELSE 0 END) + (CASE WHEN nRs > 0 THEN 1 ELSE 0 END) +
                (CASE WHEN nCell + nSta > 0 THEN 1 ELSE 0 END) AS score   -- จำนวนชนิดเงื่อนไข
        FROM    R
        WHERE   (nSt = 0 OR mSt > 0)
          AND   (nOk = 0 OR mOk > 0)
          AND   (nRp = 0 OR mRp > 0)
          AND   (nRs = 0 OR mRs > 0)
          AND   (     (nCell = 0 AND nSta = 0)
                  OR  (loc_logic = 'OR'  AND (mCell > 0 OR mSta > 0))
                  OR  (loc_logic = 'AND' AND (nCell = 0 OR mCell > 0) AND (nSta = 0 OR mSta > 0)) )
    ),
    BEST AS (
        SELECT TOP 1 * FROM M ORDER BY priority DESC, score DESC, rule_id ASC
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

/* ---------- 5) Storage_List : Status / ผู้รับผิดชอบ (เดิม → ใหม่) ตอน AAD Confirm ---------- */
/*  Old_* = snapshot จาก Tool_Equipment_List_View ตอนกดยืนยัน
    New_* = ค่าที่ AAD เลือกใน Modal (ระบบเลือกให้ตามกฎ Storage)            */
IF COL_LENGTH('dbo.Storage_List', 'Old_Status_Id') IS NULL
    ALTER TABLE [dbo].[Storage_List] ADD [Old_Status_Id] [int] NULL;
GO
IF COL_LENGTH('dbo.Storage_List', 'New_Status_Id') IS NULL
    ALTER TABLE [dbo].[Storage_List] ADD [New_Status_Id] [int] NULL;
GO
IF COL_LENGTH('dbo.Storage_List', 'Old_Responsible') IS NULL
    ALTER TABLE [dbo].[Storage_List] ADD [Old_Responsible] [nvarchar](50) NULL;
GO
IF COL_LENGTH('dbo.Storage_List', 'New_Responsible') IS NULL
    ALTER TABLE [dbo].[Storage_List] ADD [New_Responsible] [nvarchar](50) NULL;
GO

/* =====================================================================
   ตัวอย่างกฎ Storage (สร้างจากหน้า Equipment_Setting → "ใช้กับ : Storage")
   ---------------------------------------------------------------------
   1) Status = Spare AND OK/NG = NG AND Repair = ซ่อม   → Status Use : NG Wait Repair
   2) เหตุผล = EOL                                       → Status Use : Wait Scrap  (ตั้ง Priority สูงกว่าข้อ 1 ถ้าต้องการให้ EOL ชนะ)
   ===================================================================== */
