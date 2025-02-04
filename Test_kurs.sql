CREATE OR REPLACE PROCEDURE InsertDrugs AS
BEGIN
    FOR i IN 1..100000 LOOP
        INSERT INTO Drugs (
            DrugID,
            Name,
            Manufacturer,
            Instruction,
            Application_Features,
            Active_Substance,
            Indications,
            Contraindications,
            Dosage,
            Side_Effects
        ) VALUES (
            drugs_sequence.NEXTVAL,
            'Name ' || TO_CHAR(i),
            'Manufacturer ' ||  TO_CHAR(i),
            'Instruction ' ||  TO_CHAR(i),
            'Application_Features ' || TO_CHAR(i),
            'Active_Substance '  || TO_CHAR(i),
            'Indications ' || TO_CHAR(i),
            'Contraindications ' || TO_CHAR(i),
            'Dosage ' || TO_CHAR(i),
            'Side_Effects ' || TO_CHAR(i)
        );
    END LOOP;
    COMMIT;
END InsertDrugs;

-------------------------------------------
BEGIN
    InsertDrugs;
END;
-------------------------------------------
select * from Drugs;
-------------------------------------------
CREATE OR REPLACE FUNCTION GetDrugId (
    drugName IN VARCHAR2,
    dosage IN VARCHAR2
) RETURN NUMBER IS
    drugId NUMBER := -1;
BEGIN
    SELECT DrugID INTO drugId
    FROM Drugs
    WHERE Name = drugName AND Dosage = dosage;

    RETURN drugId;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN -1;
    WHEN OTHERS THEN
        RETURN -1;
END GetDrugId;

-------------------------------------------

DECLARE
    v_drug_id NUMBER;
BEGIN
    v_drug_id := GetDrugId('Name 4866', 'Dosage 4866');
    DBMS_OUTPUT.PUT_LINE('ID лекарства: ' || v_drug_id);
END;
-------------------------------------------

CREATE INDEX idx_drugs_name ON Drugs(Name);
CREATE INDEX idx_drugs_dosage ON Drugs(Dosage);