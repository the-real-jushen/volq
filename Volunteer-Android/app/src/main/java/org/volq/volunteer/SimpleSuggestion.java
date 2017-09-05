package org.volq.volunteer;

import android.content.ContentResolver;
import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import android.provider.SearchRecentSuggestions;

import java.util.ArrayList;
import java.util.List;

public class SimpleSuggestion extends SearchRecentSuggestions {

    private static final String QUERY = "query";
    private static final String ORDER_BY = "date DESC";

    private final List<String> mQueryList = new ArrayList<String>();

    public SimpleSuggestion(Context context, String authority, int mode) {
        super(context, authority, mode);

        Uri suggestionsUri = Uri.parse("content://" + authority + "/suggestions");

        // Get all suggestion
        ContentResolver cr = context.getContentResolver();
        Cursor cursor = cr.query(suggestionsUri, new String[]{QUERY}, null, null, ORDER_BY);
        while(cursor.moveToNext())
            mQueryList.add(cursor.getString(cursor.getColumnIndex(QUERY)));
        cursor.close();
    }

    public String[] getQueries() {
        return mQueryList.toArray(new String[mQueryList.size()]);
    }

    @Override
    public void saveRecentQuery(final String queryString, final String line2) {
        super.saveRecentQuery(queryString, line2);
        if (!mQueryList.contains(queryString))
            mQueryList.add(0, queryString);
    }

    @Override
    protected void truncateHistory(ContentResolver cr, int maxEntries) {
        super.truncateHistory(cr, maxEntries);

        if (maxEntries == 0) {
            mQueryList.clear();
        } else {
            for (int i = mQueryList.size() - 1; i >= maxEntries; i--) {
                mQueryList.remove(i);
            }
        }
    }
}
