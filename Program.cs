
public void Main()
{
    bool fireAgain = false;

    try
    {
        // 1. Read SSIS variables
        string zipPath =
            Dts.Variables["User::ZipFilePath"].Value.ToString();

        string archiveRoot =
            Dts.Variables["User::ZipArchiveRoot"].Value.ToString();

        string monthFolder =
            Dts.Variables["User::MonthFolder"].Value.ToString();

        // 2. Validate month folder (YYYYMM)
        if (!System.Text.RegularExpressions.Regex.IsMatch(
            monthFolder, @"^\d{6}$"))
        {
            throw new System.Exception(
                "Invalid month folder: " + monthFolder);
        }

        // 3. Dynamically create ZIP archive month folder
        string archiveMonthPath =
            System.IO.Path.Combine(
                archiveRoot, monthFolder);

        System.IO.Directory.CreateDirectory(
            archiveMonthPath);

        // 4. Get original ZIP filename
        string zipFileName =
            System.IO.Path.GetFileName(zipPath);

        string archiveFilePath =
            System.IO.Path.Combine(
                archiveMonthPath, zipFileName);

        // 5. Prevent overwriting existing ZIP
        if (System.IO.File.Exists(archiveFilePath))
        {
            throw new System.IO.IOException(
                "ZIP already exists in archive: " +
                archiveFilePath);
        }

        // 6. Verify source ZIP exists
        if (!System.IO.File.Exists(zipPath))
        {
            throw new System.IO.FileNotFoundException(
                "Original ZIP not found: " + zipPath);
        }

        // 7. Move original ZIP to archive
        System.IO.File.Move(
            zipPath,
            archiveFilePath);

        // 8. Log success
        Dts.Events.FireInformation(
            0,
            "ZIP Archive",
            "Successfully archived ZIP: " +
            archiveFilePath,
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
            "ZIP Archive",
            ex.ToString(),
            "",
            0);

        Dts.TaskResult =
            (int)ScriptResults.Failure;
    }
}
