using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using lab9.Models;
using System.IO;
using ReactiveUI;
using System.Reactive;

namespace lab9.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        //Logic
        Node selectedNode;
        public Node SelectedNode
        {
            get => selectedNode;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedNode, value);
                ImagePath = value.Fullpath;
            }
        }

        string imagePath;
        public string ImagePath
        {
            get => imagePath;
            set => this.RaiseAndSetIfChanged(ref imagePath, value);
        }

        //Command
        public ReactiveCommand<Unit,Unit> Prev { get; }
        public ReactiveCommand<Unit,Unit> Next { get; }

        // Important data
        public ObservableCollection<Node> Items { get; set; }
        public string RootFolder { get; set; }

        public MainWindowViewModel()
        {
            //No consider case for oversized directories
            Items = new ObservableCollection<Node>();
            Node rootNode = new Node();
            Items.Add(rootNode);
            SelectedNode = rootNode;

            var clickEnabled = this.WhenAnyValue(x => x.SelectedNode, x => (x.Parent != null && x.Parent.ImageCounter > 1));
            Prev = ReactiveCommand.Create(() => ChangeImage(-1), clickEnabled);
            Next = ReactiveCommand.Create(() => ChangeImage(1), clickEnabled);
        }

        public void OpenDialogWindow()
        {
            Items.Clear();
            Node rootNode = GetSubFolders(RootFolder);
            Items.Add(rootNode);
            SelectedNode = rootNode;
        }
        public void ChangeImage(int offset)
        {
            var folder = SelectedNode.Parent;
            if (folder != null)
            {
                foreach(Node SubFolders in folder.SubFolders)
                {
                    if (SubFolders.Fullpath == ImagePath)
                    {
                        int index = folder.SubFolders.IndexOf(SubFolders);
                        index += offset;
                        if (index < folder.SubFolders.Count - folder.ImageCounter) index = folder.SubFolders.Count - 1;
                        else if (index >= folder.SubFolders.Count) index = folder.SubFolders.Count - folder.ImageCounter;
                        SelectedNode = folder.SubFolders[index];
                        break;
                    }
                }
            }
        }

        Node GetSubFolders(string rootFolder)
        {

            Node newNode = new Node(rootFolder, null);
            int? index;
            bool I_dont_want_that = true;
            while (true)
            {
                try
                {
                    while (Directory.GetDirectories(newNode.Fullpath, "*", SearchOption.TopDirectoryOnly) is string[] subDir && I_dont_want_that)
                    {

                        foreach (string newDir in subDir)
                        {
                            Node thisnode = new Node(newDir, newNode);

                            newNode.SubFolders.Add(thisnode);
                        }
                        if (newNode.SubFolders.Count != 0 && newNode.SubFolders.Count != newNode.ImageCounter)
                            newNode = newNode.SubFolders[newNode.ImageCounter];
                        else break;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    //continue;
                }

                // Check for directory missing
                index = newNode.Parent?.SubFolders.IndexOf(newNode);
                if (index == null) break;
                index++;

                // Move logic
                newNode = newNode.Parent;
                if (newNode.SubFolders.Count > index)
                {
                    newNode = newNode.SubFolders[Convert.ToInt16(index)];
                    I_dont_want_that = true;
                }
                else if (newNode.Parent != null)
                {
                    I_dont_want_that = false;
                    continue;
                }
                else break;


            }
            return newNode;
        }

        //Unused
        public ObservableCollection<Node> GetSubFolders(Node dir)
        {
            ObservableCollection<Node> subFolders = new ObservableCollection<Node>();

            string[] subdirs = Directory.GetDirectories(dir.Fullpath, "*", SearchOption.TopDirectoryOnly);
            foreach (string subdir in subdirs)
            {
                try
                {
                    Node thisnode = new Node(subdir, dir);
                    if (Directory.GetDirectories(subdir, "*", SearchOption.TopDirectoryOnly).Length > 0)
                    {
                        ObservableCollection<Node> subfolders = GetSubFolders(thisnode);
                        thisnode.SubFolders = subfolders;
                    }
                    subFolders.Add(thisnode);
                }
                catch (UnauthorizedAccessException)
                {
                    //continue;
                }
            }
            if (subdirs.Length == 0)
                subFolders.Add(new Node(dir.Fullpath, null));

            return subFolders;
        }
    }
}
