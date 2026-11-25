

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






public void Main()
{
    bool fireAgain = false;

    try
    {
        // 1. Read SSIS variables
        string sourceFolder =
            Dts.Variables["User::ArchiveFolderPath"]
               .Value.ToString();

        string archiveRoot =
            Dts.Variables["User::CsvArchiveRoot"]
               .Value.ToString();

        string folderName =
            Dts.Variables["User::ArchiveFolderName"]
               .Value.ToString();

        sourceFolder = sourceFolder.TrimEnd(
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar);

        // 2. Validate source folder
        if (!System.IO.Directory.Exists(sourceFolder))
        {
            throw new System.IO.DirectoryNotFoundException(
                "Source folder not found: " + sourceFolder);
        }

        // 3. Get processing month from parent directory
        string monthFolder =
            System.IO.Directory.GetParent(sourceFolder).Name;

        if (!System.Text.RegularExpressions.Regex.IsMatch(
            monthFolder, @"^\d{6}$"))
        {
            throw new System.Exception(
                "Invalid processing month: " + monthFolder);
        }

        // 4. Create archive month directory
        string archiveMonthPath =
            System.IO.Path.Combine(
                archiveRoot,
                monthFolder);

        System.IO.Directory.CreateDirectory(
            archiveMonthPath);

        // 5. Build archive destination
        string destinationFolder =
            System.IO.Path.Combine(
                archiveMonthPath,
                folderName);

        // 6. Prevent overwriting existing folders
        if (System.IO.Directory.Exists(destinationFolder))
        {
            throw new System.IO.IOException(
                "Archive folder already exists: " +
                destinationFolder);
        }

        // 7. Move entire extracted folder
        System.IO.Directory.Move(
            sourceFolder,
            destinationFolder);

        // 8. Log archive success
        Dts.Events.FireInformation(
            0,
            "CSV Folder Archive",
            "Successfully archived: " +
            sourceFolder + " -> " + destinationFolder,
            "",
            0,
            ref fireAgain);

        Dts.TaskResult =
            (int)ScriptResults.Success;
    }
    catch (System.Exception ex)
    {
        Dts.Events.FireError(
            0,
            "CSV Folder Archive",
            ex.ToString(),
            "",
            0);

        Dts.TaskResult =
            (int)ScriptResults.Failure;
    }
}



