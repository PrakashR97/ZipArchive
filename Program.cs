

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
