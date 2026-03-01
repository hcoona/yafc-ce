using Yafc.Model;
using Yafc.UI;

namespace Yafc;

/// <summary>
/// Provides a <see cref="ImGui"/> extension overload that accepts and returns <see cref="SearchQuery"/>
/// so that callers in the application layer do not need to manually unwrap/rewrap the query string.
/// </summary>
internal static class ImGuiSearchQueryExtensions {
    public static bool BuildSearchBox(this ImGui gui, SearchQuery searchQuery, out SearchQuery newQuery, string? placeholder = null, SetKeyboardFocus setKeyboardFocus = SetKeyboardFocus.No) {
        if (gui.BuildSearchBox(searchQuery.query, out string newText, placeholder, setKeyboardFocus)) {
            newQuery = new SearchQuery(newText);
            return true;
        }

        newQuery = searchQuery;
        return false;
    }
}
