using Sales_Tracker.Classes;
using System.Reflection;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Allows the theme of all open forms to be updated without having to reinitiate the form 
    /// or manually calling the 'UpdateTheme' method for every form.
    /// </summary>
    public static class FormThemeManager
    {
        private static readonly List<Form> _openForms = [];

        public static void RegisterForm(Form form)
        {
            if (!_openForms.Contains(form))
            {
                _openForms.Add(form);
                form.FormClosed += (s, e) => _openForms.Remove(form);
            }
        }

        /// <summary>
        /// Updates the theme of all registered and open forms.
        /// </summary>
        public static void UpdateAllForms()
        {
            foreach (Form form in _openForms)
            {
                if (form != null && !form.IsDisposed)
                {
                    // First try to call UpdateTheme if it exists
                    MethodInfo updateMethod = form.GetType().GetMethod("UpdateTheme",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (updateMethod != null)
                        // Form has UpdateTheme method, so use it
                        updateMethod.Invoke(form, null);
                    else
                        // Form doesn't have UpdateTheme, so apply theme directly
                        Theme.SetThemeForForm(form);
                }
            }
        }
    }
}