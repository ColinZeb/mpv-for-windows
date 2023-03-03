
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Shell;
using MaterialDesignThemes.Wpf;

namespace mpvnet
{
    public class MenuHelp
    {
        public static MenuItem Add(ItemCollection items, string path, string icon = null)
        {
            //PackIconKind.OpenInApp
            string[] a = path.Split(new[] { " > ", " | " }, StringSplitOptions.RemoveEmptyEntries);
            ItemCollection itemCollection = items;

            for (int x = 0; x < a.Length; x++)
            {
                bool found = false;

                foreach (MenuItem i in itemCollection.OfType<MenuItem>())
                {
                    if (x < a.Length - 1)
                    {
                        if ((string)i.Header == a[x])
                        {
                            found = true;
                            itemCollection = i.Items;

                            SetIcon(icon, i);
                        }
                    }
                }

                if (!found)
                {
                    if (x == a.Length - 1)
                    {
                        if (a[x] == "-")
                            itemCollection.Add(new Separator());
                        else
                        {
                            MenuItem item = new MenuItem() { Header = a[x] };
                            itemCollection.Add(item);
                            itemCollection = item.Items;
                            SetIcon(icon, item);
                            return item;
                        }
                    }
                    else
                    {
                        MenuItem item = new MenuItem() { Header = a[x] };
                        itemCollection.Add(item);
                        itemCollection = item.Items;
                        SetIcon(icon, item);
                    }
                }
            }
            return null;
        }

        private static void SetIcon(string icon, MenuItem item)
        {
            if (icon != null)
            {
                if (Enum.TryParse(icon, out PackIconKind pk))
                {

                    item.Icon = new PackIcon { Kind = pk };
                }
            }
        }
    }
}
