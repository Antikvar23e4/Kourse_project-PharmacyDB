CREATE PUBLIC SYNONYM T_ROLES FOR Programmer.Roles;
CREATE PUBLIC SYNONYM T_USERS FOR Programmer.Users;
CREATE PUBLIC SYNONYM T_DRUGS FOR Programmer.Drugs;
CREATE PUBLIC SYNONYM T_INSTITUTION FOR Programmer.Institutions;
CREATE PUBLIC SYNONYM T_AVAILABILITY FOR Programmer.Availability;
CREATE PUBLIC SYNONYM T_RESERVATION FOR Programmer.Reservations;
CREATE PUBLIC SYNONYM T_REVIEWS FOR Programmer.Reviews;
CREATE PUBLIC SYNONYM T_REPRESENTATIVES FOR Programmer.Pharmacy_representatives;
CREATE PUBLIC SYNONYM T_MANAGERS FOR Programmer.Managers;
CREATE PUBLIC SYNONYM T_FAVORITES FOR Programmer.Favorites;
----------------------------------------------------------------------
CREATE SEQUENCE user_sequence
    START WITH 1
    INCREMENT BY 1
    NOMAXVALUE;
    
CREATE SEQUENCE manager_sequence
    START WITH 1
    INCREMENT BY 1
    NOMAXVALUE;

CREATE SEQUENCE drugs_sequence
    START WITH 1
    INCREMENT BY 1
    NOMAXVALUE;

CREATE SEQUENCE pharmacy_sequence
    START WITH 1
    INCREMENT BY 1
    NOMAXVALUE;

CREATE SEQUENCE pharmacy_representatives_sequence
    START WITH 1
    INCREMENT BY 1
    NOMAXVALUE;
    
CREATE SEQUENCE availability_sequence
    START WITH 1
    INCREMENT BY 1
    NOMAXVALUE;

CREATE SEQUENCE review_sequence
    START WITH 1
    INCREMENT BY 1
    NOMAXVALUE;
 
CREATE SEQUENCE favorite_sequence
    START WITH 1
    INCREMENT BY 1
    NOMAXVALUE;   

CREATE SEQUENCE reservation_sequence
    START WITH 1
    INCREMENT BY 1
    NOMAXVALUE;   
    
-------------------------------------------------------------------
CREATE UNIQUE INDEX idx_unique_user_data
ON T_Users(First_Name, Phone_Number, Email);

CREATE UNIQUE INDEX idx_unique_email
ON T_Users(Email);

-------------------------------------------------------------------

CREATE OR REPLACE VIEW Drug_Search_View AS
SELECT * FROM T_DRUGS;

CREATE OR REPLACE VIEW Drug_Availability_View AS
SELECT 
    D.Name AS Drug_Name,
    D.Dosage,
    I.Pharmacy_Name,
    I.Pharma_Adress AS Pharmacy_Address,
    A.Quantity,
    A.Price
FROM 
    T_DRUGS D
JOIN 
    T_AVAILABILITY A ON D.DrugID = A.DrugID
JOIN 
    T_INSTITUTION I ON A.PharmacyID = I.PharmacyID
ORDER BY 
    A.Price;

CREATE VIEW DrugReviewsView AS
SELECT 
    d.Name AS DrugName,
    d.Dosage,
    u.First_Name || ' ' ||  u.Last_Name AS UserName,
    r.Content AS ReviewContent
FROM 
    T_DRUGS d
JOIN 
    T_REVIEWS r ON d.DrugID = r.DrugID
JOIN 
    T_USERS u ON r.UserID = u.UserId;

CREATE VIEW UserFavoritesView AS
SELECT D.DrugID, D.Name, D.Dosage, D.Manufacturer, D.Instruction, F.UserID
FROM T_DRUGS D
JOIN T_FAVORITES F ON D.DrugID = F.DrugID;

CREATE VIEW UserReservationView AS
SELECT R.OrderID,
       D.Name AS DrugName,
       D.Dosage,
       P.Pharmacy_Name AS PharmacyName,
       P.Pharma_Adress AS PharmacyAddress,
       R.Order_Date,
       R.Quantity,
       R.Status,
       R.UserID
FROM T_DRUGS D
JOIN T_RESERVATION R ON D.DrugID = R.DrugID
JOIN T_INSTITUTION P ON R.PharmacyID = P.PharmacyID;

-------------------------------------------------------------------
create or replace PROCEDURE Add_Availability (
    p_DrugID IN NUMBER,
    p_PharmacyID IN NUMBER,
    p_Quantity IN NUMBER,
    p_Price IN NUMBER,
    p_error OUT VARCHAR2
)
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM T_AVAILABILITY
    WHERE DrugID = p_DrugID AND PharmacyID = p_PharmacyID;
    IF v_count > 0 THEN
        p_error := 'Запись с такими значениями DrugID и PharmacyID уже существует.';
    ELSE
        INSERT INTO T_AVAILABILITY (InventoryID, DrugID, PharmacyID, Quantity, Price)
        VALUES (availability_sequence.NEXTVAL, p_DrugID, p_PharmacyID, p_Quantity, p_Price);
        COMMIT;
        p_error := 'Данные успешно добавлены в таблицу Availability.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при добавлении данных в таблицу Availability: ' || SQLERRM;
END;

create or replace PROCEDURE Add_Drug (
    p_Name VARCHAR2,
    p_Manufacturer VARCHAR2,
    p_Instruction VARCHAR2,
    p_Application_Features VARCHAR2,
    p_Active_Substance VARCHAR2,
    p_Indications VARCHAR2,
    p_Contraindications VARCHAR2,
    p_Dosage VARCHAR2,
    p_Side_Effects VARCHAR2,
    p_result OUT VARCHAR2
) AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM T_DRUGS
    WHERE Name = p_Name AND Dosage = p_Dosage;

    IF v_count > 0 THEN
        p_result := 'Лекарство с таким же названием и дозировкой уже существует.';
    ELSE
        INSERT INTO T_DRUGS (DrugID, Name, Manufacturer, Instruction, Application_Features, Active_Substance, Indications, Contraindications, Dosage, Side_Effects)
        VALUES (drugs_sequence.NEXTVAL, p_Name, p_Manufacturer, p_Instruction, p_Application_Features, p_Active_Substance, p_Indications, p_Contraindications, p_Dosage, p_Side_Effects);
        COMMIT;
        p_result := 'Лекарство успешно добавлено.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 'Ошибка добавления лекарства: ' || SQLERRM;
END;

create or replace PROCEDURE Add_Institution (
    p_Pharmacy_Name IN VARCHAR2,
    p_Location_Latitude IN NUMBER,
    p_Location_Longitude IN NUMBER,
    p_Pharma_Adress IN VARCHAR2,
    p_Phone_Number IN VARCHAR2,
    p_Email IN VARCHAR2,
    p_Working_Hours IN VARCHAR2,
    p_error OUT VARCHAR2
) AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM T_INSTITUTION
    WHERE Pharmacy_Name = p_Pharmacy_Name
    AND Pharma_Adress = p_Pharma_Adress;

    IF v_count > 0 THEN
        p_error := 'Запись с таким названием и адресом уже существует.';
    ELSE
        IF NOT REGEXP_LIKE(p_Phone_Number, '^(\+375|80)(29|25|44|33)\d{7}$') THEN
            p_error := 'Неправильный формат номера телефона.';
            RETURN;
        END IF;
        IF NOT REGEXP_LIKE(p_Email, '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$') THEN
            p_error := 'Неправильный формат адреса электронной почты.';
            RETURN;
        END IF;
        IF NOT REGEXP_LIKE(p_Pharma_Adress, '^ул\.[[:alpha:]ёЁіІїЇґҐА-Яа-я0-9\s.,-]+ \d+$') THEN
            p_error := 'Неправильный формат адреса улицы.';
            RETURN;
        END IF;
        INSERT INTO T_INSTITUTION (PharmacyID, Pharmacy_Name, Location, Pharma_Adress, Phone_Number, Email, Working_Hours)
        VALUES (pharmacy_sequence.NEXTVAL, p_Pharmacy_Name, SDO_GEOMETRY(2001, 8307, SDO_POINT_TYPE(p_Location_Latitude, p_Location_Longitude, NULL), NULL, NULL), p_Pharma_Adress, p_Phone_Number, p_Email, p_Working_Hours);
        COMMIT;
        p_error := 'Запись успешно добавлена.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при добавлении записи: ' || SQLERRM;
END;

create or replace PROCEDURE Add_Manager (
    p_UserID IN NUMBER,
    p_Status IN VARCHAR2
)
AS
BEGIN
    DECLARE
        v_Count NUMBER;
    BEGIN
        SELECT COUNT(*)
        INTO v_Count
        FROM T_MANAGERS
        WHERE UserID = p_UserID;
        IF v_Count > 0 THEN
            RETURN;
        END IF;
    END;
    INSERT INTO T_MANAGERS (ManagerID, UserID, Status)
    VALUES (manager_sequence.NEXTVAL, p_UserID, p_Status);
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END Add_Manager;

create or replace PROCEDURE Add_Pharmacy_Representative (
    p_UserID IN NUMBER,
    p_PharmacyID IN NUMBER,
    p_error OUT VARCHAR2
) AS
BEGIN
    INSERT INTO T_REPRESENTATIVES (RepresentativeID, UserID, PharmacyID)
    VALUES (pharmacy_representatives_sequence.NEXTVAL, p_UserID, p_PharmacyID);
    COMMIT;
    p_error := 'Представитель аптеки успешно добавлен.';
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при добавлении представителя аптеки: ' || SQLERRM;
END;

create or replace  PROCEDURE Add_Reservation (
    p_UserID IN NUMBER,
    p_DrugID IN NUMBER,
    p_PharmacyID IN NUMBER,
    p_Quantity IN NUMBER,
    p_Order_Date IN DATE,
    p_Status IN VARCHAR2,
    p_OrderID OUT NUMBER,
    p_Error OUT VARCHAR2
) AS
    v_AvailableQuantity NUMBER;
BEGIN
    SELECT Quantity INTO v_AvailableQuantity
    FROM T_AVAILABILITY
    WHERE DrugID = p_DrugID AND PharmacyID = p_PharmacyID;
    IF v_AvailableQuantity IS NULL OR v_AvailableQuantity < p_Quantity THEN
        p_Error := 'Нельзя забронировать больше, чем есть в наличии.';
        RETURN; 
    END IF;
    INSERT INTO T_RESERVATION (OrderID, UserID, DrugID, PharmacyID, Quantity, Order_Date, Status)
    VALUES (reservation_sequence.NEXTVAL, p_UserID, p_DrugID, p_PharmacyID, p_Quantity, p_Order_Date, p_Status)
    RETURNING OrderID INTO p_OrderID;
    p_Error := 'Бронь успешно добавлена.';
EXCEPTION
    WHEN OTHERS THEN
        p_Error := 'Ошибка при добавлении брони: ' || SQLERRM;
END;

create or replace  PROCEDURE Add_Review (
    p_UserID IN NUMBER,
    p_DrugID IN NUMBER,
    p_Content IN VARCHAR2,
    p_error OUT VARCHAR2
) AS
    v_review_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_review_count
    FROM T_REVIEWS
    WHERE UserID = p_UserID
    AND DrugID = p_DrugID;

    IF v_review_count > 0 THEN
        p_error := 'Отзыв от этого пользователя на это лекарство уже существует.';
    ELSE
        INSERT INTO T_REVIEWS (ReviewID, UserID, DrugID, Content)
        VALUES (review_sequence.NEXTVAL, p_UserID, p_DrugID, p_Content);
        COMMIT;
        p_error := 'Отзыв успешно добавлен.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при добавлении отзыва: ' || SQLERRM;
END;

create or replace PROCEDURE Add_To_Favorites (
    p_DrugId IN NUMBER,
    p_UserId IN NUMBER,
    p_FavoriteId OUT NUMBER,
    p_Error OUT VARCHAR2
) AS
    v_Count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_Count
    FROM T_FAVORITES
    WHERE DrugId = p_DrugId AND UserId = p_UserId;
    IF v_Count > 0 THEN
        SELECT FavoriteID INTO p_FavoriteId
        FROM T_FAVORITES
        WHERE DrugId = p_DrugId AND UserId = p_UserId;

        p_Error := 'Запись уже существует в избранном.';
    ELSE
        INSERT INTO T_FAVORITES (FavoriteID, DrugId, UserID)
        VALUES (favorite_sequence.NEXTVAL, p_DrugId, p_UserId)
        RETURNING FavoriteID INTO p_FavoriteId;

        p_Error := 'Запись успешно добавлена в избранное.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        p_Error := 'Ошибка при добавлении записи в избранное: ' || SQLERRM;
END;

create or replace PROCEDURE Authenticate_User (
    p_Username IN VARCHAR2,
    p_Password IN VARCHAR2,
    p_SuccessMsg OUT VARCHAR2,
    p_ErrorMsg OUT VARCHAR2
)
AS
    v_UserCount NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_UserCount
    FROM T_USERS
    WHERE First_Name = p_Username
    AND Password = p_Password;
    IF v_UserCount = 1 THEN
        p_SuccessMsg := 'Аутентификация успешна. Добро пожаловать, ' || p_Username || '.';
    ELSE
        p_ErrorMsg := 'Ошибка аутентификации. Пользователь с указанными учетными данными не найден.';
        p_SuccessMsg := null;
    END IF;
END Authenticate_User;

create or replace PROCEDURE Delete_Availability (
    p_DrugID IN NUMBER,
    p_PharmacyID IN NUMBER,
    p_error OUT VARCHAR2
)
AS
BEGIN
    DELETE FROM T_AVAILABILITY
    WHERE DrugID = p_DrugID AND PharmacyID = p_PharmacyID;
    COMMIT;
    p_error := 'Запись успешно удалена из таблицы Availability.';
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при удалении записи из таблицы Availability: ' || SQLERRM;
END;

create or replace PROCEDURE Delete_Drug (
    p_Name VARCHAR2,
    p_Dosage VARCHAR2,
    p_result OUT VARCHAR2
) AS
BEGIN
    DELETE FROM T_DRUGS
    WHERE Name = p_Name
        AND Dosage = p_Dosage;

    IF SQL%ROWCOUNT = 1 THEN
        COMMIT;
        p_result := 'Лекарство успешно удалено.';
    ELSE
        ROLLBACK;
        p_result := 'Не удалось найти лекарство с указанным названием и дозировкой.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 'Ошибка при удалении лекарства: ' || SQLERRM;
END;

create or replace PROCEDURE Delete_Favorite(
    p_UserId IN NUMBER,
    p_DrugId IN NUMBER,
    p_Error OUT VARCHAR2
) AS
BEGIN
    DELETE FROM T_FAVORITES
    WHERE UserId = p_UserId
    AND DrugId = p_DrugId;

    IF SQL%ROWCOUNT = 1 THEN
        p_Error := 'Запись в избранном успешно удалена.';
    ELSE
        p_Error := 'Запись в избранном не найдена.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        p_Error := 'Ошибка при удалении записи из избранного: ' || SQLERRM;
END;

create or replace PROCEDURE Delete_Institution (
    p_Pharmacy_Name IN VARCHAR2,
    p_Pharma_Adress IN VARCHAR2,
    p_error OUT VARCHAR2
) AS
BEGIN
    DELETE FROM T_INSTITUTION
    WHERE Pharmacy_Name = p_Pharmacy_Name
    AND Pharma_Adress = p_Pharma_Adress;

    IF SQL%ROWCOUNT = 1 THEN
        COMMIT;
        p_error := 'Аптека успешно удалена.';
    ELSE
        ROLLBACK;
        p_error := 'Аптека с указанным названием и адресом не найдена.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при удалении аптеки: ' || SQLERRM;
END;

create or replace PROCEDURE Delete_Reservation (
    p_OrderID IN NUMBER,
    p_error OUT VARCHAR2
) AS
BEGIN
    DELETE FROM T_RESERVATION
    WHERE OrderID = p_OrderID;

    IF SQL%ROWCOUNT = 1 THEN
        COMMIT;
        p_error := 'Запись о бронировании успешно удалена.';
    ELSE
        ROLLBACK;
        p_error := 'Не удалось найти запись о бронировании с указанным OrderID.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при удалении записи о бронировании: ' || SQLERRM;
END;

create or replace PROCEDURE Delete_User (
    p_UserId IN Users.UserId%TYPE
)
IS
BEGIN
    DELETE FROM Users WHERE UserId = p_UserId;
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Пользователь успешно удален.');
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Пользователь с указанным UserId не найден.');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Ошибка при удалении пользователя: ' || SQLERRM);
END Delete_User;

create or replace PROCEDURE Find_Closest_Pharmacies(
    user_latitude IN NUMBER,
    user_longitude IN NUMBER,
    cur_pharmacies OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN cur_pharmacies FOR
    SELECT 
        PharmacyID,
        Pharmacy_Name,
        Pharma_Adress,
        Working_Hours,
        ROUND(
            SDO_GEOM.SDO_DISTANCE(
                Location,
                SDO_GEOMETRY(2001, 8307, SDO_POINT_TYPE(user_longitude, user_latitude, NULL), NULL, NULL),
                0.5
            ),
            2
        ) AS distance
    FROM T_INSTITUTION
    ORDER BY distance;
END;

create or replace PROCEDURE Get_User_Id (
    p_Username IN VARCHAR2,
    p_Password IN VARCHAR2,
    p_UserId OUT NUMBER
)
AS
BEGIN
    SELECT UserId INTO p_UserId
    FROM T_USERS
    WHERE First_name = p_Username
    AND Password = p_Password;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_UserId := NULL;
END Get_User_Id;

create or replace PROCEDURE Get_User_RoleId (
    p_Username IN VARCHAR2,
    p_Password IN VARCHAR2,
    p_RoleId OUT NUMBER
)
AS
BEGIN
    SELECT RoleId INTO p_RoleId
    FROM T_USERS
    WHERE First_name = p_Username
    AND Password = p_Password;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_RoleId := NULL;
END Get_User_RoleId;

create or replace PROCEDURE Register_User (
    p_First_Name IN VARCHAR2,
    p_Last_Name IN VARCHAR2,
    p_Middle_Name IN VARCHAR2,
    p_Phone_Number IN VARCHAR2,
    p_Email IN VARCHAR2,
    p_RoleId IN NUMBER,
    p_Password IN VARCHAR2,
    p_ErrorMsg OUT VARCHAR2
)
AS
    v_UserId NUMBER;
    v_Count NUMBER;
BEGIN
    IF NOT REGEXP_LIKE(p_Phone_Number, '^(\+375|80)(29|25|44|33)\d{7}$') THEN
        p_ErrorMsg := 'Неправильный формат номера телефона';
        RETURN;
    END IF;
    IF REGEXP_LIKE(p_Email, '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$') = FALSE THEN
        p_ErrorMsg := 'Неправильный формат адреса электронной почты';
        RETURN;
    END IF;
    SELECT COUNT(*) INTO v_Count
    FROM T_Users
    WHERE Phone_Number = p_Phone_Number
    OR Email = p_Email;
    IF v_Count > 0 THEN
        p_ErrorMsg := 'Пользователь с таким номером телефона или адресом электронной почты уже существует';
    ELSE
        IF NOT (
            INITCAP(p_First_Name) = p_First_Name
            AND INITCAP(p_Last_Name) = p_Last_Name
            AND INITCAP(p_Middle_Name) = p_Middle_Name
        ) THEN
            p_ErrorMsg := 'Имя, фамилия и отчество должны начинаться с большой буквы';
            RETURN;
        END IF;
        SELECT user_sequence.nextval INTO v_UserId FROM dual;
        INSERT INTO T_Users (UserId, First_Name, Last_Name, Middle_Name, Phone_Number, Email, RoleId, Password)
        VALUES (v_UserId, p_First_Name, p_Last_Name, p_Middle_Name, p_Phone_Number, p_Email, p_RoleId, p_Password);
        COMMIT;
        p_ErrorMsg := 'Вы успешно зарегистрировались';
    END IF;
END Register_User;

create or replace PROCEDURE SearchPharmacy (
    p_name IN VARCHAR2,
    p_address IN VARCHAR2,
    p_pharmacy_id OUT NUMBER,
    p_error_msg OUT VARCHAR2
)
IS
BEGIN
    SELECT PharmacyID
    INTO p_pharmacy_id
    FROM T_INSTITUTION
    WHERE Pharmacy_Name = p_name
    AND Pharma_Adress = p_address;

    p_error_msg := NULL; 
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_pharmacy_id := NULL;
        p_error_msg := 'Аптека с именем ' || p_name || ' и адресом ' || p_address || ' не найдена.';
END;

create or replace PROCEDURE Update_Availability (
    p_DrugID IN NUMBER,
    p_PharmacyID IN NUMBER,
    p_Quantity IN NUMBER,
    p_Price IN NUMBER,
    p_error OUT VARCHAR2
) AS
BEGIN
    UPDATE T_AVAILABILITY
    SET Quantity = p_Quantity,
        Price = p_Price
    WHERE DrugID = p_DrugID
    AND PharmacyID = p_PharmacyID;

    COMMIT;
    p_error := 'Запись в таблице Availability успешно обновлена.';
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при обновлении записи в таблице Availability: ' || SQLERRM;
END;

create or replace PROCEDURE Update_Drug (
    p_Name VARCHAR2,
    p_Dosage VARCHAR2,
    p_Manufacturer VARCHAR2,
    p_Instruction VARCHAR2,
    p_Application_Features VARCHAR2,
    p_Active_Substance VARCHAR2,
    p_Indications VARCHAR2,
    p_Contraindications VARCHAR2,
    p_Side_Effects VARCHAR2,
    p_result OUT VARCHAR2
) AS
BEGIN
    UPDATE T_DRUGS
    SET Manufacturer = p_Manufacturer,
        Instruction = p_Instruction,
        Application_Features = p_Application_Features,
        Active_Substance = p_Active_Substance,
        Indications = p_Indications,
        Contraindications = p_Contraindications,
        Side_Effects = p_Side_Effects
    WHERE Name = p_Name
        AND Dosage = p_Dosage;

    IF SQL%ROWCOUNT = 1 THEN
        COMMIT;
        p_result := 'Информация о лекарстве успешно обновлена.';
    ELSE
        ROLLBACK;
        p_result := 'Не удалось найти лекарство с указанным названием и дозировкой.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 'Ошибка при обновлении информации о лекарстве: ' || SQLERRM;
END;

create or replace PROCEDURE Update_Institution (
    p_Pharmacy_Name IN VARCHAR2,
    p_Pharma_Adress IN VARCHAR2,
    p_New_Phone_Number IN VARCHAR2,
    p_New_Email IN VARCHAR2,
    p_New_Working_Hours IN VARCHAR2,
    p_error OUT VARCHAR2
) AS
BEGIN

    UPDATE T_INSTITUTION
    SET Phone_Number = p_New_Phone_Number,
        Email = p_New_Email,
        Working_Hours = p_New_Working_Hours
    WHERE Pharmacy_Name = p_Pharmacy_Name
    AND Pharma_Adress = p_Pharma_Adress;
    IF SQL%ROWCOUNT = 1 THEN
        COMMIT;
        p_error := 'Информация об аптеке успешно обновлена.';
    ELSE
        ROLLBACK;
        p_error := 'Не удалось найти аптеку с указанным названием и адресом.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при обновлении информации об аптеке: ' || SQLERRM;
END;

create or replace PROCEDURE Update_Manager_Status (
    p_UserID IN NUMBER,
    p_Status IN VARCHAR2
)
AS
BEGIN
    UPDATE T_MANAGERS
    SET Status = p_Status
    WHERE UserID = p_UserID;

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END Update_Manager_Status;

create or replace PROCEDURE Update_Pharmacy_Representative (
    p_UserID IN NUMBER,
    p_PharmacyID IN NUMBER,
    p_error OUT VARCHAR2
)
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM T_REPRESENTATIVES
    WHERE UserID = p_UserID;

    IF v_count > 0 THEN
        UPDATE T_REPRESENTATIVES 
        SET PharmacyID = p_PharmacyID 
        WHERE UserID = p_UserID;
        COMMIT;
        p_error := 'Представитель аптеки успешно обновлен.';
    ELSE
        p_error := 'Представитель аптеки с указанным UserID не найден.';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при обновлении представителя аптеки: ' || SQLERRM;
END;

create or replace PROCEDURE Update_User_Info (
    p_UserID IN NUMBER,
    p_PhoneNumber IN VARCHAR2,
    p_Email IN VARCHAR2,
    p_Password IN VARCHAR2,
    p_error OUT VARCHAR2
)
AS
BEGIN
    UPDATE T_USERS
    SET Phone_Number = p_PhoneNumber,
        Email = p_Email,
        Password = p_Password
    WHERE UserId = p_UserID;

    COMMIT;
    p_error := 'Данные пользователя успешно обновлены.';
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_error := 'Пользователь с указанным UserID не найден.';
    WHEN OTHERS THEN
        ROLLBACK;
        p_error := 'Ошибка при обновлении данных пользователя: ' || SQLERRM;
END;

CREATE OR REPLACE PROCEDURE UpdateReservationStatus (
    p_OrderID IN NUMBER,
    p_NewStatus IN VARCHAR2,
    p_Error OUT VARCHAR2
) AS
    v_ValidStatus BOOLEAN;
    v_CurrentStatus VARCHAR2(50);
BEGIN
    SELECT Status INTO v_CurrentStatus
    FROM Reservations
    WHERE OrderID = p_OrderID;
    IF v_CurrentStatus = 'Одобрено' THEN
        p_Error := 'Статус уже "Одобрено" и не может быть изменен.';
    ELSE
        IF p_NewStatus IN ('Одобрено', 'Отклонено') THEN
            UPDATE Reservations
            SET Status = p_NewStatus
            WHERE OrderID = p_OrderID;
            IF SQL%ROWCOUNT = 1 THEN
                p_Error := 'Статус успешно обновлен.';
            ELSE
                p_Error := 'Не удалось обновить статус.';
            END IF;
        ELSE
            p_Error := 'Недопустимое значение статуса. Допустимые значения: Одобрено, Отклонено.';
        END IF;
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_Error := 'Бронь с указанным OrderID не найдена.';
    WHEN OTHERS THEN
        p_Error := 'Ошибка при обновлении статуса: ' || SQLERRM;
END;