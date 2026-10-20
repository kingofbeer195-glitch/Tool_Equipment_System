/*  แจ้งเปลี่ยนเครื่องมือวัด (change_type = 'gauge') : Flow 28 QC Check -> 31 Approve 2
    เอกสารที่สร้างก่อนเปลี่ยน flow บันทึกผู้อนุมัติ J3 เป็น status_id = 30 -> ปรับเป็น 31
    และเอกสารที่ค้างสถานะ 30 (flow เดิม) -> ย้ายไป 31 ให้ J3 อนุมัติต่อได้  */
USE [tool_eduipment_system]
GO

UPDATE A SET A.status_id = 31
FROM NotificationofChange_Approver A
JOIN NotificationofChange_Control_List C ON C.Doc_No = A.Doc_No
WHERE C.change_type = 'gauge' AND A.status_id = 30;
GO

UPDATE NotificationofChange_Control_List SET status_id = 31
WHERE change_type = 'gauge' AND status_id = 30;

UPDATE L SET L.status_id = 31
FROM NotificationofChange_List L
JOIN NotificationofChange_Control_List C ON C.Doc_No = L.Doc_No
WHERE C.change_type = 'gauge' AND L.status_id = 30;
GO
