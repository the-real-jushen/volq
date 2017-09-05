package org.volq.volunteer;

import android.content.SearchRecentSuggestionsProvider;

public class SimpleSuggestionProvider extends SearchRecentSuggestionsProvider {
    public final static String AUTHORITY = "org.volq.volunteer.SimpleSuggestionProvider";
    public final static int MODE = DATABASE_MODE_QUERIES;

    public SimpleSuggestionProvider() {
        setupSuggestions(AUTHORITY, MODE);
    }
}
