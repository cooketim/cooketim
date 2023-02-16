using System.Collections.Generic;
using System.Linq;

namespace Models.ViewModels
{
    public abstract class SearchableTreeViewModel : ViewModelBase
    {
        #region Search Logic
        IEnumerator<TreeViewItemViewModel> matchingEnumerator;
        string enumeratorSearchText;
        public string SearchErrorText { get; set; }

        private protected string staticSearchErrorText { get; set; }

        public virtual void PerformSearch(SilentObservableCollection<TreeViewItemViewModel> items, string searchText)
        {
            SearchErrorText = null;
            if (matchingEnumerator == null || (!string.IsNullOrEmpty(enumeratorSearchText) && enumeratorSearchText != searchText))
            {
                this.VerifyMatchingEnumerator(items, searchText);
            }
            else
            {
                var advanced = matchingEnumerator.MoveNext();
                if (!advanced)
                {
                    if (!string.IsNullOrEmpty(enumeratorSearchText) && enumeratorSearchText == searchText)
                    {
                        matchingEnumerator.Reset();
                        if (!matchingEnumerator.MoveNext())
                        {
                            SearchErrorText = string.Format(staticSearchErrorText, searchText);
                        }
                    }
                    else
                    {
                        this.VerifyMatchingEnumerator(items, searchText);
                    }
                }
            }

            var item = matchingEnumerator.Current;

            if (item == null)
                return;

            // Ensure that the item is in view.
            if (item.Parent != null)
                item.Parent.IsExpanded = true;

            if (item.IsSelected)
            {
                item.IsSelected = false;
            }

            item.IsSelected = true;
        }

        private void VerifyMatchingEnumerator(SilentObservableCollection<TreeViewItemViewModel> items, string searchText)
        {
            var matches = FindMatches(searchText, items).ToList();

            //dedupe the matches
            var groups = matches
                           .GroupBy(x =>
                            new
                            {
                                x.TreeViewItemViewModel.ParentId,
                                x.TreeViewItemViewModel.Id
                            });
            var duplicates = groups.Where(x => x.Count() > 1);

            foreach (var item in duplicates)
            {
                var toProcess = item.OrderBy(x => x.SearchRank);
                for (var i = 1; i < toProcess.Count(); i++)
                {
                    matches.Remove(toProcess.ElementAt(i));
                }
            }

            //first get the root parents
            var searchResults = new List<TreeViewItemViewModel>();
            var roots = matches.Where(x => string.IsNullOrEmpty(x.TreeViewItemViewModel.ParentId)).OrderBy(x => x.SearchRank).ThenBy(x => x.TreeViewItemViewModel.Label).Select(x => x.TreeViewItemViewModel);
            var children = matches.Where(x => !string.IsNullOrEmpty(x.TreeViewItemViewModel.ParentId)).Select(x => x.TreeViewItemViewModel).ToList();
            
            //add the root items to the search results
            foreach (var root in roots)
            {
                searchResults.Add(root);
                RecursivelyAddChildren(matches, searchResults, root, children);
            }

            //deal with any remaining children
            if (children.Any())
            {
                searchResults.AddRange(children);
            }

            matchingEnumerator = searchResults.GetEnumerator();

            if (!matchingEnumerator.MoveNext())
            {
                SearchErrorText = string.Format(staticSearchErrorText,searchText);
            }

            enumeratorSearchText = searchText;
        }

        private void RecursivelyAddChildren(List<RankedTreeViewItemViewModel> matches, List<TreeViewItemViewModel> searchResults, TreeViewItemViewModel parent, List<TreeViewItemViewModel> allChildren)
        {
            //get any children
            var children = matches.Where(x => !string.IsNullOrEmpty(x.TreeViewItemViewModel.ParentId) && string.Equals(parent.Id, x.TreeViewItemViewModel.ParentId)).OrderBy(x => x.SearchRank).ThenBy(x => x.TreeViewItemViewModel.Label).Select(x => x.TreeViewItemViewModel);
            foreach (var child in children)
            {
                searchResults.Add(child);
                //remove from teh collection of all children
                allChildren.Remove(child);
                RecursivelyAddChildren(matches, searchResults, child, allChildren);
            }
        }

        private IEnumerable<RankedTreeViewItemViewModel> FindMatches(string searchText, SilentObservableCollection<TreeViewItemViewModel> items)
        {
            foreach (var item in items)
            {
                var rootId = item.RootId;

                //search by text
                var res = item.NameContainsText(searchText);
                
                if (res.Item1)
                {
                    yield return new RankedTreeViewItemViewModel(item, res.Item2);
                }

                if (item.Children != null)
                {
                    foreach (var child in FindMatches(searchText, item.Children))
                    {
                        yield return child;
                    }
                }
            }
        }

        #endregion // Search Logic
    }
}
