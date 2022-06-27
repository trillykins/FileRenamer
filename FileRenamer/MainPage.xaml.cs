using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FileRenamer;

public partial class MainPage : ContentPage
{
    private string _renameFileButtonText;

    public MainPage()
    {
        InitializeComponent();
        _renameFileButtonText = RenameFilesButton.Text;
    }

    private void OnDirectoryClicked(object sender, EventArgs e)
    {
        ListFiles(SelectedDirectory.Text);
        ModifyFiles(SelectedDirectory.Text, false);

        SemanticScreenReader.Announce(OriginalFilenames.Text);
        SemanticScreenReader.Announce(ModifiedFilenames.Text);
    }

    private void OnRenameFilesClicked(object sender, EventArgs e)
    {
        ListFiles(SelectedDirectory.Text);
        ModifyFiles(SelectedDirectory.Text, true);

        SemanticScreenReader.Announce(OriginalFilenames.Text);
        SemanticScreenReader.Announce(ModifiedFilenames.Text);
    }

    // Spend effort moving this? meh maybe
    private void ModifyFiles(string path, bool renameFiles)
    {
        if (path == null || !Directory.Exists(path))
        {
            Debug.WriteLine("Returning!");
            return;
        }
        var filenames = new List<(string, string)>();
        foreach (var str in Directory.GetFiles(path))
        {
            var file = new FileInfo(str);
            var newName = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
            if (!file.Exists) continue;

            if (!string.IsNullOrEmpty(RemoveFrom.Text))
            {
                var index = newName.IndexOf(RemoveFrom.Text);
                if (index != -1)
                {
                    newName = file.Name.Remove(index);
                }
            }

            if (!string.IsNullOrEmpty(RegExr.Text) && !string.IsNullOrEmpty(AddAfterRegex.Text))
            {
                var regex = new Regex(RegExr.Text);
                var regexMatch = regex.Match(newName);
                if (regexMatch.Success)
                {
                    var index = file.Name.IndexOf(regexMatch.Value);
                    if (index != -1)
                    {
                        index += regexMatch.Value.Length;
                        newName = newName.Insert(index, AddAfterRegex.Text);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Replace.Text) && !string.IsNullOrEmpty(ReplaceWith.Text))
            {
                if (CaseInsensitive.IsToggled)
                {
                    newName = newName.Replace(Replace.Text, ReplaceWith.Text, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    newName = newName.Replace(Replace.Text, ReplaceWith.Text);
                }
            }

            if (TrimTrailingWhitespace.IsToggled)
            {
                newName = newName.TrimEnd();
            }

            newName = $"{newName}{file.Extension}";
            filenames.Add((file.FullName, newName));
        }
        ModifiedFilenames.Text = string.Join(Environment.NewLine, filenames.Select(x => x.Item2));
        // Block button
        Debug.WriteLine(filenames.Select(x => x.Item2).Count());
        Debug.WriteLine(filenames.Count);
        var canRenameFiles = filenames.Select(x => x.Item2).Distinct().Count() == filenames.Count;
        Debug.WriteLine($"{nameof(canRenameFiles)}: {canRenameFiles}");
        if (canRenameFiles)
        {
            RenameFilesButton.IsEnabled = true;
            RenameFilesButton.Text = _renameFileButtonText;
        }
        else
        {
            RenameFilesButton.IsEnabled = false;
            RenameFilesButton.Text = "Cannot rename with duplicate filenames!";
        }

        if (renameFiles && canRenameFiles)
        {
            foreach (var file in filenames)
            {
                //File.Move(Path.Combine(path, file.Item1), Path.Combine(path, file.Item2));
            }
        }
    }

    private void ListFiles(string path)
    {
        if (path == null || !Directory.Exists(path))
        {
            Debug.WriteLine("Returning!");
            return;
        }
        var filenames = new List<string>();
        foreach (var str in Directory.GetFiles(path))
        {
            var file = new FileInfo(str);
            if (!file.Exists) continue;
            filenames.Add(file.Name);
        }
        OriginalFilenames.Text = string.Join(Environment.NewLine, filenames);
    }

    private void TrimTrailingWhitespace_Toggled(object sender, ToggledEventArgs e)
    {

    }
}

