/*  แจ้งเปลี่ยนเครื่องมือวัด (NotificationOfChange : change_type = 'gauge')
    ขั้น 1 = 28 QC Check  → ขั้น 2 = 30 J3 Up → 50 AAD Confirm

    1) เพิ่ม status 28 ใน Master_Status (ใช้ทั้ง badge สถานะ และหน้า Authorize : sys_change = 1)
    2) ตั้งรายชื่อ QC ในหน้า Authorize → Process AAD2 → Step 'QC Check'

    *** ตรวจคอลัมน์ NOT NULL อื่นของ Master_Status ก่อนรัน (เช่น sys_borrow / sys_withdraw ฯลฯ) ***  */
USE [tool_eduipment_system]
GO

IF NOT EXISTS (SELECT 1 FROM Master_Status WHERE status_id = 28)
BEGIN
    INSERT INTO Master_Status (status_id, status_name, Style_CSS, sys_change, status_use)
    SELECT 28, 'QC Check', Style_CSS, 1, 1
    FROM Master_Status WHERE status_id = 30;     -- ใช้สี badge เดียวกับ J3 Up
END
GO

/* (ทางเลือก) ถ้าต้องการให้ชื่อ status 30 เป็น 'J3 Up Approve 2' — มีผลกับเอกสารทุกโหมด
UPDATE Master_Status SET status_name = 'J3 Up Approve 2' WHERE status_id = 30;
*/
