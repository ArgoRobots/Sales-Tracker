using System.Reflection;

namespace Sales_Tracker.Classes
{
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
                    {
                        // Form doesn't have UpdateTheme, so apply theme directly
                        Theme.SetThemeForForm(form);
                    }
                }
            }
        }
    }
}