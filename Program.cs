

public void Main()
{
    try
    {
        string root =
            Dts.Variables["User::CsvRootFolder"]
               .Value.ToString();

        if (!System.IO.Directory.Exists(root))
        {
            throw new System.IO.DirectoryNotFoundException(
                "CSV folder not found: " + root);
        }

        System.Data.DataTable folderTable =
            new System.Data.DataTable();

        folderTable.Columns.Add(
            "FolderPath",
            typeof(string));

        string[] folders =
            System.IO.Directory.GetDirectories(root);

        foreach (string folder in folders)
        {
            folderTable.Rows.Add(folder);
        }

        Dts.Variables["User::ArchiveFolderList"].Value =
            folderTable;

        Dts.TaskResult = (int)ScriptResults.Success;
    }
    catch (System.Exception ex)
    {
        Dts.Events.FireError(
            0,
            "CSV Archive Folder List",
            ex.ToString(),
            "",
            0);

        Dts.TaskResult = (int)ScriptResults.Failure;
    }
}



SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM FR_SANDBOX.CSV_LOAD_LOG
        WHERE SOURCE_FOLDER_NAME = ?
          AND STATUS = 'SUCCESS'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM FR_SANDBOX.CSV_LOAD_LOG
        WHERE SOURCE_FOLDER_NAME = ?
          AND STATUS IN ('PROCESSING', 'FAILED')
          AND NOT EXISTS (
              SELECT 1
              FROM FR_SANDBOX.CSV_LOAD_LOG S
              WHERE S.SOURCE_FOLDER_NAME =
                    CSV_LOAD_LOG.SOURCE_FOLDER_NAME
                AND S.CSV_FILE_NAME =
                    CSV_LOAD_LOG.CSV_FILE_NAME
                AND S.STATUS = 'SUCCESS'
          )
    )
    THEN 1
    ELSE 0
END AS CAN_ARCHIVE
FROM DUAL


