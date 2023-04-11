using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using CommunityToolkit.Maui.Storage;

namespace FileRenamer;

public partial class MainPage : ContentPage
{
    private readonly string _renameFileButtonText;

    // < (less than)
    // > (greater than)
    // : (colon - sometimes works, but is actually NTFS Alternate Data Streams)
    // " (double quote)
    // / (forward slash)
    // \ (backslash)
    // | (vertical bar or pipe)
    // ? (question mark)
    // * (asterisk)
    private readonly char[] _illegalCharacters = new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

    public MainPage()
    {
        InitializeComponent();
        _renameFileButtonText = RenameFilesButton.Text;
    }


    private void OnDirectoryClicked(object sender, EventArgs e)
    {
        DoTheThings(false);
    }

    private void OnRenameFilesClicked(object sender, EventArgs e)
    {
        DoTheThings(true);
    }

    // TODO: rename and refactor
    private void DoTheThings(bool renameFiles)
    {
        ListFiles(SelectedDirectory.Text);
        ModifyFiles(SelectedDirectory.Text, renameFiles);

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
            var newName = file.Name[..^file.Extension.Length];
            if (!file.Exists) continue;

            if (!string.IsNullOrEmpty(RemoveFrom.Text))
            {
                var index = newName.IndexOf(RemoveFrom.Text);
                if (CaseInsensitiveRemoveFrom.IsToggled)
                {
                    index = newName.IndexOf(RemoveFrom.Text, StringComparison.OrdinalIgnoreCase);
                }
                if (index != -1)
                {
                    newName = file.Name.Remove(index);
                }
            }

            if (!string.IsNullOrEmpty(RegExr.Text) && !string.IsNullOrEmpty(AddAfterRegex.Text))
            {
                try
                {
                    var regex = new Regex(RegExr.Text);
                    var regexMatch = regex.Match(newName);
                    if (regexMatch.Success)
                    {
                        newName = newName.Insert(regexMatch.Index + regexMatch.Value.Length, AddAfterRegex.Text);
                    }
                }
                catch (ArgumentException e)
                {
                    Debug.WriteLine($"Invalid regex: {e.Message}");
                }
            }

            if (!string.IsNullOrEmpty(Replace.Text))
            {
                if (CaseInsensitive.IsToggled)
                {
                    newName = newName.Replace(Replace.Text, ReplaceWith.Text ?? "", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    newName = newName.Replace(Replace.Text, ReplaceWith.Text ?? "");
                }
            }

            if (TrimLeadingWhitespace.IsToggled)
            {
                newName = newName.TrimStart();
            }

            if (TrimTrailingWhitespace.IsToggled)
            {
                newName = newName.TrimEnd();
            }

            newName = $"{newName}{file.Extension}";
            filenames.Add((file.FullName, newName));
        }

        ModifiedFilenames.Text = string.Join(Environment.NewLine, filenames.Select(x => x.Item2));

        var canRenameFiles = filenames.Select(x => x.Item2).Distinct().Count() == filenames.Count;
        if (canRenameFiles)
        {
            // Verify that no modified filenames contain illegal characters
            if (filenames.Select(x => x.Item2).Any(x => _illegalCharacters.Any(c => x.Contains(c))))
            {
                RenameFilesButton.IsEnabled = false;
                RenameFilesButton.Text = $"Cannot rename files containing illegal characters ({string.Join(" ", _illegalCharacters)})";
                canRenameFiles = false;
            }
            else
            {
                RenameFilesButton.IsEnabled = true;
                RenameFilesButton.Text = _renameFileButtonText;
                canRenameFiles = true;
            }
        }
        else
        {
            RenameFilesButton.IsEnabled = false;
            RenameFilesButton.Text = "Cannot rename with duplicate filenames!";
            canRenameFiles = false;
        }

        if (renameFiles && canRenameFiles)
        {
            foreach (var file in filenames)
            {
                File.Move(Path.Combine(path, file.Item1), Path.Combine(path, file.Item2));
            }
            ListFiles(path);
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

    private async void OnPickFolderClicked(object sender, EventArgs e)
    {
        var path = SelectedDirectory.Text + "\\";
        path = path.Replace("\\", "/");
        Debug.WriteLine(path);
        var result = await FolderPicker.PickAsync(path, CancellationToken.None);
        if (result.IsSuccessful)
        {
            SelectedDirectory.Text = result.Folder.Path;
            SemanticScreenReader.Announce(SelectedDirectory.Text);
            DoTheThings(false);
        }

    }
}

