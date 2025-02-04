CREATE OR REPLACE DIRECTORY XML_FILES AS 'C:\WINDOWS.X64_193000_db_home\database\xml_files';
SELECT * FROM all_directories WHERE directory_name = 'XML_FILES';
GRANT READ, WRITE ON DIRECTORY XML_FILES TO Programmer;

-------------------------------------------

CREATE OR REPLACE FUNCTION ReadFileXML(p_file_path IN VARCHAR2) RETURN CLOB IS
  v_file_handle UTL_FILE.FILE_TYPE;
  v_file_content CLOB;
  v_buffer VARCHAR2(32767);
BEGIN
  v_file_handle := UTL_FILE.FOPEN('XML_FILES', p_file_path, 'R');
  LOOP
    BEGIN
      UTL_FILE.GET_LINE(v_file_handle, v_buffer);
      v_file_content := v_file_content || v_buffer;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        EXIT;
    END;
  END LOOP;
  UTL_FILE.FCLOSE(v_file_handle);
  RETURN v_file_content;
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Error reading file XML: ' || SQLERRM);
    RETURN NULL;
END;

-------------------------------------------

CREATE OR REPLACE FUNCTION WriteToFileXML(p_file_path IN VARCHAR2, p_xml_content IN CLOB) RETURN BOOLEAN IS
   v_file UTL_FILE.FILE_TYPE;
BEGIN
  v_file := UTL_FILE.FOPEN('XML_FILES', p_file_path, 'W');
  UTL_FILE.PUT_LINE(v_file, p_xml_content);
  UTL_FILE.FCLOSE(v_file);
  RETURN TRUE;
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Error reading file XML: ' || SQLERRM);
    RETURN FALSE;
END;

----------------------------------------

CREATE OR REPLACE PROCEDURE ImportDataFromXML(
    p_file_path IN VARCHAR2,
    p_table_name IN VARCHAR2
) IS
    v_file_content CLOB;
    v_xml XMLTYPE;
    v_columns VARCHAR2(10000);
    v_query VARCHAR2(10000);
v_file_handle UTL_FILE.FILE_TYPE;
v_buffer VARCHAR2(32767);

BEGIN
v_file_content := ReadFileXML(p_file_path);
    v_xml := XMLTYPE(v_file_content);
SELECT LISTAGG(column_name || ' ' || 
               CASE 
                  WHEN data_type = 'NUMBER' THEN 'INT'
                  WHEN data_type = 'VARCHAR2' THEN 'VARCHAR(255)'
                  ELSE data_type
               END || ' PATH ''' || column_name || '''', ',')
WITHIN GROUP (ORDER BY column_id)
INTO v_columns
FROM all_tab_columns
WHERE table_name = UPPER(p_table_name);
    DBMS_OUTPUT.PUT_LINE(v_columns);
    v_query := '
    INSERT INTO ' || p_table_name || '
    SELECT *
    FROM
        XMLTable(''/ROWSET/ROW''
            PASSING XMLTYPE(''' || v_xml.getClobVal() || ''')
            COLUMNS ' || v_columns || '
            )';

    DBMS_OUTPUT.PUT_LINE(v_query);
    EXECUTE IMMEDIATE v_query;

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Data successfully imported from XML file to table ' || p_table_name || '.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Error importing data from XML to table ' || p_table_name || ': ' || SQLERRM);
        ROLLBACK;
END;

-------------------------------------------
BEGIN
    ImportDataFromXML(p_file_path => 'platCopy.xml', p_table_name => 'DRUGS');
END;
-------------------------------------------

CREATE OR REPLACE PROCEDURE ExportDataToXML(
    p_file_path IN VARCHAR2,
    p_table_name IN VARCHAR2
) IS
    v_xml_data CLOB;
    v_success BOOLEAN;
BEGIN
    EXECUTE IMMEDIATE '
        SELECT DBMS_XMLGEN.GETXML(''SELECT * FROM ' || p_table_name || ''') FROM DUAL'
        INTO v_xml_data;

    v_success := WriteToFileXML(p_file_path, v_xml_data);
    IF v_success THEN
        DBMS_OUTPUT.PUT_LINE('Table ' || p_table_name || ' data successfully written to XML file.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Error occurred while writing ' || p_table_name || ' data to XML file.');
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Error exporting data from table ' || p_table_name || ' to XML: ' || SQLERRM);
END;

-------------------------------------------

BEGIN
    ExportDataToXML(p_file_path => 'platCopy.xml', p_table_name => 'DRUGS');
END;

-------------------------------------------
