CREATE OR REPLACE TRIGGER Register_User_Trigger
AFTER INSERT ON T_Users
FOR EACH ROW
DECLARE
    v_ErrorMsg VARCHAR2(200);
BEGIN
    IF :NEW.UserId IS NULL THEN
        v_ErrorMsg := 'Ошибка при регистрации';
    ELSE
        v_ErrorMsg := 'Вы успешно зарегистрировались';
    END IF;

    DBMS_OUTPUT.PUT_LINE(v_ErrorMsg);
END;

CREATE OR REPLACE TRIGGER Update_Availability2
AFTER UPDATE OF Status ON T_RESERVATION
FOR EACH ROW
BEGIN
    IF :OLD.Status <> 'Одобрено' AND :NEW.Status = 'Одобрено' THEN
        UPDATE T_AVAILABILITY
        SET Quantity = Quantity - :NEW.Quantity
        WHERE DrugID = :NEW.DrugID AND PharmacyID = :NEW.PharmacyID;
    END IF;
END;

CREATE OR REPLACE TRIGGER Update_Availability
AFTER DELETE ON T_RESERVATION
FOR EACH ROW
DECLARE
    PRAGMA AUTONOMOUS_TRANSACTION;
    v_Quantity NUMBER;
BEGIN
    SELECT Quantity INTO v_Quantity
    FROM T_RESERVATION
    WHERE OrderID = :OLD.OrderID;
    IF :OLD.Status = 'Одобрено' THEN
        UPDATE T_AVAILABILITY
        SET Quantity = Quantity + v_Quantity
        WHERE DrugID = :OLD.DrugID AND PharmacyID = :OLD.PharmacyID;
    END IF;
    COMMIT;
END;
