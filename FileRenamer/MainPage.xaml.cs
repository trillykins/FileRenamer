using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FileRenamer;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        Debug.WriteLine(TrimTrailingWhitespace.IsToggled);
        ListFiles(SelectedDirectory.Text);
        ModifyFiles(SelectedDirectory.Text);

        SemanticScreenReader.Announce(OriginalFilenames.Text);
        SemanticScreenReader.Announce(ModifiedFilenames.Text);
    }

    private void ModifyFiles(string path)
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
            var newName = file.Name;
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

            //special cases
            //if (file.FullName.ToLower().Contains("bobs") && file.FullName.ToLower().Contains("burgers"))
            //{
            //    newName = newName.Replace("Bobs", "Bob's");
            //}

            newName = newName.Replace(".", " ");
            if (TrimTrailingWhitespace.IsToggled)
            {
                newName = newName.TrimEnd();
            }
            newName = $"{newName}{file.Extension}";
            filenames.Add(newName);
        }
        ModifiedFilenames.Text = string.Join(Environment.NewLine, filenames);
        //File.Move(Path.Combine(path, file.FullName), Path.Combine(path, newName));
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
}

