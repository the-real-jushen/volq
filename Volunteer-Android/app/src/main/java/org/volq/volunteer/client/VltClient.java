package org.volq.volunteer.client;

import android.content.Context;
import android.graphics.BitmapFactory;
import android.os.AsyncTask;
import android.support.annotation.NonNull;
import android.webkit.MimeTypeMap;

import com.alibaba.fastjson.JSON;
import com.hippo.network.HttpHelper;
import com.hippo.network.ResponseCodeException;
import com.hippo.network.UrlBuilder;
import com.hippo.util.ArrayUtils;
import com.hippo.util.Base64;
import com.hippo.util.Coordinate;
import com.hippo.util.JSONUtils;
import com.hippo.util.Log;
import com.hippo.util.MathUtils;
import com.hippo.util.TextUtils;
import com.hippo.util.Utils;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import org.volq.volunteer.R;
import org.volq.volunteer.account.VltAccount;
import org.volq.volunteer.account.VltAccountStore;
import org.volq.volunteer.data.ApplyFromMe;
import org.volq.volunteer.data.ApplyToMe;
import org.volq.volunteer.data.Badge;
import org.volq.volunteer.data.CheckUpdateInfo;
import org.volq.volunteer.data.Comment;
import org.volq.volunteer.data.Doing;
import org.volq.volunteer.data.Feed;
import org.volq.volunteer.data.Friend;
import org.volq.volunteer.data.Summary;
import org.volq.volunteer.data.User;
import org.volq.volunteer.data.ValidateImage;
import org.volq.volunteer.data.Volunteer;
import org.volq.volunteer.data.VolunteersRecord;
import org.volq.volunteer.network.VltHttpHelper;
import org.volq.volunteer.util.VltUtils;

import java.io.File;
import java.io.UnsupportedEncodingException;
import java.net.HttpURLConnection;
import java.net.URLEncoder;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.TimeZone;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

// TODO Richer error check
public final class VltClient {
    private static final String TAG = VltClient.class.getSimpleName();

    private static final String EXCEPTION_ERROR_RESULT = "Error result";
    private static final String EXCEPTION_UNAUTHORIZED = "Unauthorized";
    private static final String EXCEPTION_WTF = "WTF";

    public static final int PAGE_SIZE = 25;

    public static final String API_HEADER = "http://www.volq.org"; //http://115.156.252.231:80"; //

    private static final String API_CHECK_UPDATE = "/api/mobileapp/androidapp?version=";

    private static final String API_LOGIN = "/api/user/login";
    private static final String API_USER = "/api/user/information?id=";
    private static final String API_VOLUNTEER = "/api/volunteer/statistics?id=";
    private static final String API_FEED = "/api/user/myfeeds";
    private static final String API_MY_FRIENDS = "/api/volunteer/myfriends?id=";
    private static final String API_RECOMMEND_FRIENDS = "/api/volunteer/recommendfriend?number=25&id=";

    private static final String API_FAVORITE = "/api/volunteer/favorite";

    private static final String API_SEARCH_NOT_MY_FRIEND = "/api/volunteer/searchnotmyfriendbyfilter";
    private static final String API_MY_RANK = "/api/volunteer/myrank";
    private static final String API_MY_FRIENDS_RANK = "/api/volunteer/myfriendsrank";
    private static final String API_APPLY_TO_ME_HISTORY = "/api/volunteer/applytomehistory";
    private static final String API_APPLY_FROM_ME_HISTORY = "/api/volunteer/applyfrommehistory";
    private static final String API_MY_DOINGS = "/api/activity/mine";
    private static final String API_DOINGS = "/api/activity";
    private static final String API_DOING = "/api/activity?id=";
    private static final String API_HOT_TAGS = "/api/activity/hottags?number=" + 7;
    private static final String API_VALIDATE_IMAGE = "/api/user/validateimage";
    private static final String API_REGISTER = "/api/user/register";
    private static final String API_IS_MY_FRIEND = "/api/volunteer/ismyfriend?id=";
    private static final String API_APPLY_FRIEND = "/api/volunteer/applyfriend";

    private static final String API_ACTIVITY_IS_FAVORITE = "/api/activity/isfavorited";
    private static final String API_ACTIVITY_FAVORITE = "/api/volunteer/favorite";
    private static final String API_ACTIVITY_UNFAVORITE = "/api/volunteer/unfavorite";

    private static final String API_VOLUNTEER_ACTION = "/api/volunteer/action";

    private static final String API_REFUSE_FRIEND = "/api/volunteer/refusefriend";
    private static final String API_ACCEPT_FRIEND = "/api/volunteer/acceptfriend";

    private static final String API_USER_BADGES = "/api/badge/userbadges";
    private static final String API_USER_BADGE_DETAIL = "/api/badge/userbadgedetail";

    private static final String API_SIGN_IN_DOING = "/api/volunteer/signinactivity";
    private static final String API_SIGN_OUT_DOING = "/api/volunteer/signoutactivity";

    private static final String API_UPLOAD_AVATAR = "/api/content/uploadavatar";

    private static final String API_UPDATE_EMAIL = "/api/user/email";
    private static final String API_UPDATE_PHONE = "/api/user/phonenumber";
    private static final String API_UPDATE_AFFILIATION = "/api/volunteer/affiliation";
    private static final String API_UPDATE_DESCRIPTION = "/api/user/description";

    private static final String API_COMMENT = "/api/activity/comment";
    private static final String API_SUMMARY = "/api/activity/summary";
    private static final String API_RATE = "/api/activity/rate";

    private static final String API_KEY_ID = "id";
    private static final String API_KEY_FILTER_SOURCE = "filtersource";
    private static final String API_KEY_SORT_BY_KEY = "sortbykey";
    private static final String API_KEY_IS_ASCENDING = "isascending";
    private static final String API_KEY_PAGE_INDEX = "pageindex";
    private static final String API_KEY_PAGE_SIZE = "pagesize";
    private static final String API_KEY_BADGE_NAME = "badgename";
    private static final String API_KEY_STAGE = "stage";
    private static final String API_KEY_TYPE = "type";

    private static final String API_KEY_EMAIL = "email";
    private static final String API_KEY_FRIEND_NAME = "friendname";
    private static final String API_KEY_AFFILIATION = "affiliation";

    public static final String DOING_STAGE_ALL = "all";
    public static final String DOING_STAGE_ABOUT_TO_START = "aboutToStart";
    public static final String DOING_STAGE_RUNNING = "running";
    public static final String DOING_STAGE_FINISH = "finish";

    public static final String COMMENT_TYPE_ACTIVITY = "activity";
    public static final String COMMENT_TYPE_SUMMARY = "summary";

    private static final DateFormat sRobotDateFormat1 = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", Locale.ENGLISH);
    private static final DateFormat sRobotDateFormat2 = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", Locale.ENGLISH);
    private static final DateFormat sHumanDateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.ENGLISH); // TODO a better format

    static {
        sRobotDateFormat1.setTimeZone(TimeZone.getTimeZone("GMT"));
        sRobotDateFormat2.setTimeZone(TimeZone.getTimeZone("GMT"));
        sHumanDateFormat.setTimeZone(TimeZone.getDefault());
    }

    private Context mContext;
    private VltAccountStore mAccountStore;

    private VltClient(Context context) {
        mContext = context;
        mAccountStore = VltAccountStore.getInstance(context);
    }

    private static VltClient sInstance;

    public static VltClient getInstance(Context context) {
        if (sInstance == null) {
            sInstance = new VltClient(context.getApplicationContext());
        }
        return sInstance;
    }

    /**
     * For api which do need return
     */
    private static final ReturnObject RETURE_OBJECT = new ReturnObject();

    private static final class ReturnObject {
        // Empty
    }

    public static Date parserDate1(String dateStr) throws ParseException {
        return sRobotDateFormat1.parse(dateStr);
    }

    public static Date parserDate2(String dateStr) throws ParseException {
        return sRobotDateFormat2.parse(dateStr);
    }

    public static Date parserDateSafely1(String dateStr) {
        try {
            return sRobotDateFormat1.parse(dateStr);
        } catch (Exception e) {
            return new Date(0);
        }
    }

    public static Date parserDateSafely2(String dateStr) {
        try {
            return sRobotDateFormat2.parse(dateStr);
        } catch (Exception e) {
            return new Date(0);
        }
    }


    public static String formatDateToRobot1(Date date) {
        return sRobotDateFormat1.format(date);
    }

    public static String formatDateToRobot2(Date date) {
        return sRobotDateFormat2.format(date);
    }

    // TODO Show time like 1 hour ago ?
    public static String formatDateToHuman(Date date) {
        return sHumanDateFormat.format(date);
    }

    private Coordinate readCoordinate(String raw) {
        if (raw == null) {
            return null;
        }

        Pattern p = Pattern.compile("lng:(-?\\d+.\\d+),lat:(-?\\d+.\\d+)");
        Matcher m = p.matcher(raw);
        if (m.find()) {
            try {
                return new Coordinate(Double.parseDouble(m.group(1)),
                        Double.parseDouble(m.group(2)));
            } catch (NumberFormatException e) {
                return null;
            }
        } else {
            return null;
        }
    }

    public static String rightUrl(String url) {
        if (url == null) {
            return null;
        }

        String[] pieces = url.split("/");
        StringBuilder sb = new StringBuilder(API_HEADER);
        for (String p : pieces) {
            if (android.text.TextUtils.isEmpty(p)) {
                continue;
            }
            sb.append('/');
            try {
                sb.append(URLEncoder.encode(p, "utf-8").replace("+", "%20"));
            } catch (UnsupportedEncodingException e) {
                e.printStackTrace();
            }
        }

        return sb.toString();
    }

    public static void rightUrlArray(String[] urlArray) {
        if (urlArray == null) {
            return;
        }

        int length = urlArray.length;
        for (int i = 0; i < length; i++) {
            urlArray[i] = rightUrl(urlArray[i]);
        }
    }


    private Feed readFeed(JSONObject jo) throws JSONException, ParseException {
        Feed feed = new Feed();
        feed.id = jo.getString("Id");
        feed.title = jo.getString("Title");
        feed.text = jo.getString("Text");
        feed.pictures = JSONUtils.getStringArray(jo, "Pictures");
        feed.destinationLink = jo.getString("DestinationLink");
        feed.time = parserDateSafely1(jo.getString("Time"));
        feed.fromId = jo.getString("MessageFrom");
        feed.fromName = jo.getString("FromName");
        feed.fromAvatar = jo.getString("FromAvatar");

        // Fix
        rightUrlArray(feed.pictures);
        // Don't fix it
        // feed.destinationLink = rightUrl(feed.destinationLink);
        feed.fromAvatar = rightUrl(feed.fromAvatar);

        return feed;
    }

    /**
     * id, name, avatar(AvatarPath), description
     */
    private Friend readFriend1(JSONObject jo) throws JSONException, ParseException {
        Friend friend = new Friend();
        friend.id = jo.getString("id");
        friend.name = jo.getString("name");
        friend.avatar = jo.getJSONObject("avatar").getString("AvatarPath");
        friend.description = jo.getString("description");

        // Fix
        friend.avatar = rightUrl(friend.avatar);

        return friend;
    }

    /**
     * id, name, avatar(AvatarPath), description, level
     */
    private Friend readFriend2(JSONObject jo) throws JSONException, ParseException {
        Friend friend = readFriend1(jo);
        friend.level = jo.getInt("level");

        return friend;
    }

    /**
     * id, email, name, affiliation, activityCount, badgeCount
     */
    private Friend readFriend3(JSONObject jo) throws JSONException, ParseException {
        Friend friend = new Friend();
        friend.id = jo.getString("id");
        friend.email = jo.getString("email");
        friend.name = jo.getString("name");
        friend.affiliation = JSONUtils.getStringArray(jo, "affiliation");
        friend.point = jo.getInt("point");
        friend.activityCount = jo.getInt("activityCount");
        friend.badgeCount = jo.getInt("badgeCount");

        return friend;
    }

    private ApplyFromMe readApplyFromMe(JSONObject jo) throws JSONException, ParseException {
        ApplyFromMe applyFromMe = new ApplyFromMe();
        applyFromMe.toId = jo.getString("VolunteerId");
        applyFromMe.name = jo.getString("Name");
        applyFromMe.applyTime = parserDateSafely1(jo.getString("ApplyTime"));
        try {
            applyFromMe.actionTime = parserDate1(jo.getString("ActionTime"));
        } catch (Exception e) {
            // Empty
        }
        applyFromMe.comment = jo.getString("Comment");
        applyFromMe.status = jo.getInt("Status");

        return applyFromMe;
    }

    private ApplyToMe readApplyToMe(JSONObject jo) throws JSONException, ParseException {
        ApplyToMe applyToMe = new ApplyToMe();
        applyToMe.fromId = jo.getString("id");
        applyToMe.name = jo.getString("name");
        applyToMe.avatar = jo.getJSONObject("avatar").getString("AvatarPath");
        applyToMe.date = parserDateSafely1(jo.getString("Time"));
        applyToMe.comment = jo.getString("Comment");

        applyToMe.avatar = rightUrl(applyToMe.avatar);

        return applyToMe;
    }

    private Doing readDoing(JSONObject jo) throws JSONException, ParseException {
        Doing doing = new Doing();
        doing.Id = jo.getString("Id");
        doing.Name = jo.getString("Name");
        doing.OrganizationName = jo.getString("OrganizationName");
        doing.Point = jo.getInt("Point");
        doing.Status = jo.getInt("Status");
        doing.OpenSignInTime = parserDateSafely2(jo.getString("OpenSignInTime"));
        doing.StartTime = parserDateSafely2(jo.getString("StartTime"));
        doing.FinishTime = parserDateSafely2(jo.getString("FinishTime"));
        doing.Location = jo.getString("Location");
        doing.Coordinate = readCoordinate(jo.getString("Coordinate"));
        doing.Cover = jo.getString("Cover");
        doing.Tags = JSONUtils.getStringArray(jo, "Tags");
        doing.HasSignedInVolunteerNumber = jo.getInt("HasSignedInVolunteerNumber");
        doing.VolunteerViewedTime = jo.getInt("VolunteerViewedTime");
        doing.VolunteerFavoritedTime = jo.getInt("VolunteerFavoritedTime");
        doing.hasViewed = jo.getBoolean("hasViewed");
        doing.hasFavorited = jo.getBoolean("hasFavorited");
        doing.hasSignined = jo.getBoolean("hasSignined");

        // Fix
        doing.Cover = rightUrl(doing.Cover);

        return doing;
    }

    private Badge readBadge(JSONObject jo) throws JSONException, ParseException {
        Badge badge = new Badge();
        badge.name = jo.getString("badgeName");
        badge.picture = jo.getString("badgePicture");

        // Fix
        badge.picture = rightUrl(badge.picture);


        Log.d("TAG", "badge.picture = " + badge.picture);

        return badge;
    }

    private VolunteersRecord readVolunteersRecord(JSONObject jo) throws JSONException, ParseException {
        VolunteersRecord volunteersRecord = new VolunteersRecord();

        volunteersRecord.volunteerId = jo.getString("VolunteerId");
        volunteersRecord.volunteerName = jo.getString("VolunteerName");
        volunteersRecord.volunteerSex = jo.getInt("VolunteerSex");
        volunteersRecord.volunteerEmail = jo.getString("VolunteerEmail");
        volunteersRecord.volunteerPhoneNumber = jo.getString("VolunteerPhoneNumber");
        volunteersRecord.volunteerStatus = jo.getInt("VolunteerStatus");

        return volunteersRecord;
    }



    private void checkHttpError(VltHttpHelper vhh, String body) throws ResponseCodeException {
        final int responseCode = vhh.getResponseCode();

        if (responseCode >= 400) {
            if (body.startsWith("{")) {
                try {
                    JSONObject jo = new JSONObject(body);
                    throw new ResponseCodeException(responseCode, jo.getString("Message"));
                } catch (JSONException e) {
                    throw new ResponseCodeException(responseCode);
                }

            } else if (body.length() > 0 && !body.startsWith("<")) {
                throw new ResponseCodeException(responseCode, body);
            } else {
                throw new ResponseCodeException(responseCode);
            }
        }
    }


    /*
    private static final String API_UPDATE_EMAIL = "/api/user/email";
    private static final String API_UPDATE_PHONE = "/api/user/phonenumber";
    private static final String API_UPDATE_AFFILIATION = "/api/volunteer/affiliation";
    private static final String API_UPDATE_DESCRIPTION = "/api/user/description";
    */

    public static final int TYPE_EMAIL = 0;
    public static final int TYPE_PHONE = 1;
    public static final int TYPE_AFFILIATION = 2;
    public static final int TYPE_DESCRIPTION = 3;


    private CheckUpdateInfo doCheckUpdate(int currentVersion) throws Exception {
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String result = hh.get(API_HEADER + API_CHECK_UPDATE + currentVersion);

        checkHttpError(hh, result);

        return JSON.parseObject(result, CheckUpdateInfo.class);
    }

    private VltAccount doLogin(String email, String password)
            throws Exception {
        JSONObject jo = new JSONObject();
        jo.put("email", email);
        jo.put("password", password);
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String result = hh.postJson(API_HEADER + API_LOGIN, jo);

        checkHttpError(hh, result);

        JSONObject reJo = new JSONObject(result);

        String status = reJo.getString("status");
        if ("OK".equals(status)) {
            VltAccount account = new VltAccount();
            account.email = email;
            account.password = password;
            account.name = reJo.getString("name");
            account.role = reJo.getString("role");
            account.userId = reJo.getString("userId");
            account.token = reJo.getString("token");
            return account;
        } else if ("ERROR".equals(status)) {
            throw new VltException(reJo.getString("message"));
        } else {
            throw new VltException("Unknown status: " + status);
        }
    }

    private User doGetUser(String id)
            throws Exception {
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String result = hh.get(API_HEADER + API_USER + id);

        checkHttpError(hh, result);

        User user = new User();
        JSONObject reJo = new JSONObject(result);
        user.id = id;
        user.name = reJo.getString("name");
        user.avatar = reJo.getString("avatar");
        user.role = reJo.getInt("role");
        user.email = reJo.getString("email");
        user.description = reJo.getString("description");
        user.sex = reJo.getInt("sex");
        user.isEmailVerified = reJo.getBoolean("IsEmailVerified");
        user.phoneNumber = reJo.getString("phoneNumber");

        try {
            user.affiliation = JSONUtils.getStringArray(reJo, "affiliation");
            user.location = reJo.getString("location");
            // TODO It is a bad idea
            if (TextUtils.STRING_NULL.equals(user.location)) {
                user.location = null;
            }
            user.coordinate = readCoordinate(reJo.getString("coordinate"));
        } catch (Exception e) {
            // Empty
        }

        // Fix
        user.avatar = rightUrl(user.avatar);

        return user;
    }

    private Volunteer doGetVolunteer(@NonNull User user, String id)
            throws Exception {
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String result = hh.get(API_HEADER + API_VOLUNTEER + id);

        checkHttpError(hh, result);

        Volunteer volunteer = new Volunteer(user);
        JSONObject reJo = new JSONObject(result);
        volunteer.name = reJo.getString("name");
        volunteer.level = reJo.getInt("level");
        volunteer.levelName = reJo.getString("levelName");
        volunteer.levelPicture = reJo.getString("levelPicture");
        volunteer.point = reJo.getInt("point");
        volunteer.pointsToNextLevel = reJo.getInt("pointsToNextLevel");
        volunteer.strength = reJo.getInt("strength");
        volunteer.intelligence = reJo.getInt("intelligence");
        volunteer.endurance = reJo.getInt("endurance");
        volunteer.compassion = reJo.getInt("compassion");
        volunteer.sacrifice = reJo.getInt("sacrifice");
        volunteer.signedInActivityNumber = reJo.getInt("signedInActivityNumber");
        volunteer.completeRate = reJo.getDouble("completeRate");

        // Fix
        volunteer.levelPicture = rightUrl(volunteer.levelPicture);

        return volunteer;
    }

    /**
     * @param index Start from 0
     * @throws Exception
     */
    private Feed[] doGetFeeds(String id, int index)
            throws Exception {
        UrlBuilder ub = new UrlBuilder(API_HEADER + API_FEED);
        ub.addQuery(API_KEY_ID, id);
        ub.addQuery(API_KEY_SORT_BY_KEY, "time");
        ub.addQuery(API_KEY_IS_ASCENDING, false);
        ub.addQuery(API_KEY_PAGE_INDEX, index + 1);
        ub.addQuery(API_KEY_PAGE_SIZE, PAGE_SIZE);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String result = hh.get(ub.build());

        checkHttpError(hh, result);

        JSONArray reJa = new JSONArray(result);
        int length = reJa.length();
        List<Feed> feedLsit = new ArrayList<Feed>();
        for (int i = 0; i < length; i++) {
            try {
                feedLsit.add(readFeed(reJa.getJSONObject(i)));
            } catch (Exception e) {
                Log.d("TAG", "Get feed item error", e);
            }
        }
        return feedLsit.toArray(new Feed[feedLsit.size()]);
    }

    private Friend[] doGetMyFriends() throws Exception {
        VltAccount account = mAccountStore.getCurAccount();
        if (account == null) {
            throw new VltException(
                    mContext.getString(R.string.mesg_current_account_invaild)); // TODO
        }

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String result = hh.get(API_HEADER + API_MY_FRIENDS + account.userId);

        checkHttpError(hh, result);

        JSONArray reJa = new JSONArray(result);
        int length = reJa.length();
        List<Friend> friendLsit = new ArrayList<Friend>();
        for (int i = 0; i < length; i++) {
            try {
                friendLsit.add(readFriend2(reJa.getJSONObject(i)));
            } catch (Exception e) {
                // Empty
            }
        }
        return friendLsit.toArray(new Friend[friendLsit.size()]);
    }

    private Friend[] doGetRecommendFriends() throws Exception {
        VltAccount account = mAccountStore.getCurAccount();
        if (account == null) {
            throw new VltException(
                    mContext.getString(R.string.mesg_current_account_invaild)); // TODO
        }

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String result = hh.get(API_HEADER + API_RECOMMEND_FRIENDS + account.userId);

        checkHttpError(hh, result);

        JSONArray reJa = new JSONArray(result);
        int length = reJa.length();
        List<Friend> friendLsit = new ArrayList<Friend>();
        for (int i = 0; i < length; i++) {
            try {
                friendLsit.add(readFriend1(reJa.getJSONObject(i)));
            } catch (Exception e) {
                // Empty
            }
        }
        return friendLsit.toArray(new Friend[friendLsit.size()]);
    }

    private Friend[] doSearchNotMyFriend(String name) throws Exception {
        UrlBuilder ub = new UrlBuilder(API_HEADER + API_SEARCH_NOT_MY_FRIEND);
        ub.addQuery(API_KEY_EMAIL, TextUtils.STRING_EMPTY);
        ub.addQuery(API_KEY_FRIEND_NAME, name);
        ub.addQuery(API_KEY_AFFILIATION, TextUtils.STRING_EMPTY);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(ub.build());

        checkHttpError(hh, body);

        JSONArray reJa = new JSONArray(body);
        int length = reJa.length();
        List<Friend> friendLsit = new ArrayList<Friend>();
        for (int i = 0; i < length; i++) {
            try {
                friendLsit.add(readFriend2(reJa.getJSONObject(i)));
            } catch (Exception e) {
                // Empty
            }
        }
        return friendLsit.toArray(new Friend[friendLsit.size()]);
    }

    private void doVolunteerAction(String id)
            throws Exception {
        JSONObject jo = new JSONObject();
        jo.put("id", id);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.postJson(API_HEADER + API_VOLUNTEER_ACTION, jo);

        checkHttpError(hh, body);

        int responseCode = hh.getResponseCode();
        switch (responseCode) {
            case HttpURLConnection.HTTP_OK:
                // OK
                return;
            case -1:
                // No response code ?
                throw new VltException(EXCEPTION_WTF);
            default:
                throw new ResponseCodeException(responseCode);
        }
    }

    private int doMyRank(String key) throws Exception {
        VltAccount account = mAccountStore.getCurAccount();
        if (account == null) {
            throw new VltException(
                    mContext.getString(R.string.mesg_current_account_invaild));
        }

        UrlBuilder ub = new UrlBuilder(API_HEADER + API_MY_RANK);
        ub.addQuery(API_KEY_ID, account.userId);
        ub.addQuery(API_KEY_SORT_BY_KEY, key);
        ub.addQuery(API_KEY_IS_ASCENDING, "false");
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(ub.build());

        checkHttpError(hh, body);

        return Integer.parseInt(body);
    }

    private Friend[] doMyFriendsRank(String key, int pageIndex) throws Exception {
        VltAccount account = mAccountStore.getCurAccount();
        if (account == null) {
            throw new VltException(
                    mContext.getString(R.string.mesg_current_account_invaild));
        }

        UrlBuilder ub = new UrlBuilder(API_HEADER + API_MY_FRIENDS_RANK);
        ub.addQuery(API_KEY_ID, account.userId);
        ub.addQuery(API_KEY_SORT_BY_KEY, key);
        ub.addQuery(API_KEY_IS_ASCENDING, "false");
        ub.addQuery(API_KEY_PAGE_SIZE, PAGE_SIZE);
        ub.addQuery(API_KEY_PAGE_INDEX, pageIndex);
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(ub.build());

        checkHttpError(hh, body);

        JSONArray reJa = new JSONArray(body);
        int length = reJa.length();
        List<Friend> friendLsit = new ArrayList<Friend>(length);
        for (int i = 0; i < length; i++) {
            try {
                friendLsit.add(readFriend3(reJa.getJSONObject(i)));
            } catch (Exception e) {
                // Empty
            }
        }
        return friendLsit.toArray(new Friend[friendLsit.size()]);
    }

    private ApplyFromMe[] doApplyFromMe() throws Exception {
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(API_HEADER + API_APPLY_FROM_ME_HISTORY);

        checkHttpError(hh, body);

        JSONArray reJa = new JSONArray(body);
        int length = reJa.length();
        List<ApplyFromMe> applyFromMeList = new ArrayList<ApplyFromMe>(length);
        for (int i = 0; i < length; i++) {
            try {
                applyFromMeList.add(readApplyFromMe(reJa.getJSONObject(i)));
            } catch (Exception e) {
                // Empty
            }
        }
        return applyFromMeList.toArray(new ApplyFromMe[applyFromMeList.size()]);
    }

    private ApplyToMe[] doApplyToMe() throws Exception {
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(API_HEADER + API_APPLY_TO_ME_HISTORY);

        checkHttpError(hh, body);

        JSONArray reJa = new JSONArray(body);
        int length = reJa.length();
        List<ApplyToMe> applyToMeList = new ArrayList<>(length);
        for (int i = 0; i < length; i++) {
            try {
                applyToMeList.add(readApplyToMe(reJa.getJSONObject(i)));
            } catch (Exception e) {
                Log.w(TAG, "Read ApplyToMe Error", e);
            }
        }
        return applyToMeList.toArray(new ApplyToMe[applyToMeList.size()]);
    }

    private Doing[] doDoings(String filtersource, String stage, int pageindex) throws Exception {
        UrlBuilder ub = new UrlBuilder(API_HEADER + API_DOINGS);
        ub.addQuery(API_KEY_STAGE, stage);
        ub.addQuery(API_KEY_FILTER_SOURCE, filtersource);
        ub.addQuery(API_KEY_SORT_BY_KEY, TextUtils.STRING_EMPTY);
        ub.addQuery(API_KEY_IS_ASCENDING, "false");
        ub.addQuery(API_KEY_PAGE_INDEX, pageindex + 1);
        ub.addQuery(API_KEY_PAGE_SIZE, PAGE_SIZE);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(ub.build());

        checkHttpError(hh, body);

        JSONArray reJa = new JSONArray(body);
        int length = reJa.length();
        List<Doing> doingList = new ArrayList<Doing>(length);
        for (int i = 0; i < length; i++) {
            try {
                doingList.add(readDoing(reJa.getJSONObject(i)));
            } catch (Exception e) {
                // Empty
            }
        }
        return doingList.toArray(new Doing[doingList.size()]);
    }

    private Doing[] doMyDoings(String id, String filtersource, String stage, int pageindex) throws Exception {
        UrlBuilder ub = new UrlBuilder(API_HEADER + API_MY_DOINGS);
        ub.addQuery(API_KEY_ID, id);
        ub.addQuery(API_KEY_STAGE, stage);
        ub.addQuery(API_KEY_FILTER_SOURCE, filtersource);
        ub.addQuery(API_KEY_SORT_BY_KEY, TextUtils.STRING_EMPTY);
        ub.addQuery(API_KEY_IS_ASCENDING, "false");
        ub.addQuery(API_KEY_PAGE_INDEX, pageindex + 1);
        ub.addQuery(API_KEY_PAGE_SIZE, PAGE_SIZE);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(ub.build());

        checkHttpError(hh, body);

        JSONArray reJa = new JSONArray(body);
        int length = reJa.length();
        List<Doing> doingList = new ArrayList<Doing>(length);
        for (int i = 0; i < length; i++) {
            try {
                doingList.add(readDoing(reJa.getJSONObject(i)));
            } catch (Exception e) {
                // Empty
            }
        }
        return doingList.toArray(new Doing[doingList.size()]);
    }

    private Doing[] doGetFavoriteDoings(String filtersource, String stage, int pageindex) throws Exception {
        VltAccount account = mAccountStore.getCurAccount();
        if (account == null) {
            throw new VltException(
                    mContext.getString(R.string.mesg_current_account_invaild));
        }

        UrlBuilder ub = new UrlBuilder(API_HEADER + API_FAVORITE);
        ub.addQuery(API_KEY_ID, account.userId);
        ub.addQuery(API_KEY_STAGE, stage);
        ub.addQuery(API_KEY_FILTER_SOURCE, filtersource);
        ub.addQuery(API_KEY_SORT_BY_KEY, TextUtils.STRING_EMPTY);
        ub.addQuery(API_KEY_IS_ASCENDING, "false");
        ub.addQuery(API_KEY_PAGE_INDEX, pageindex + 1);
        ub.addQuery(API_KEY_PAGE_SIZE, PAGE_SIZE);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(ub.build());

        checkHttpError(hh, body);

        JSONArray reJa = new JSONArray(body);
        int length = reJa.length();
        List<Doing> doingList = new ArrayList<Doing>(length);
        for (int i = 0; i < length; i++) {
            try {
                doingList.add(readDoing(reJa.getJSONObject(i)));
            } catch (Exception e) {
                // Empty
            }
        }
        return doingList.toArray(new Doing[doingList.size()]);
    }

    private Doing doDoing(String id) throws Exception {
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(API_HEADER + API_DOING + id);

        checkHttpError(hh, body);

        JSONObject reJo = new JSONObject(body);
        Doing doing = new Doing();
        doing.Id = reJo.getString("Id");
        doing.OrganizerId = reJo.getString("OrganizerId");
        doing.OrganizationId = reJo.getString("OrganizationId");
        doing.OrganizationName = reJo.getString("OrganizationName");
        doing.Name = reJo.getString("Name");
        doing.Abstract = reJo.getString("Abstract");
        doing.ActivateTime = parserDateSafely1(reJo.getString("ActivateTime"));
        doing.OpenSignInTime = parserDateSafely2(reJo.getString("OpenSignInTime"));
        doing.CloseSignInTime = parserDateSafely2(reJo.getString("CloseSignInTime"));
        doing.StartTime = parserDateSafely2(reJo.getString("StartTime"));
        doing.FinishTime = parserDateSafely2(reJo.getString("FinishTime"));
        doing.Point = reJo.getInt("Point");
        doing.MostVolunteers = reJo.getInt("MostVolunteers");
        doing.LeastVolunteers = reJo.getInt("LeastVolunteers");
        doing.Location = reJo.getString("Location");
        doing.Coordinate = readCoordinate(reJo.getString("Coordinate"));
        doing.Procedure = reJo.getString("Procedure");
        doing.Photos = JSONUtils.getStringArray(reJo, "Photos");
        doing.Videos = JSONUtils.getStringArray(reJo, "Videos");
        doing.Status = reJo.getInt("Status");
        doing.Tags = JSONUtils.getStringArray(reJo, "Tags");
        doing.HexagramPropertyTotalPointLimit = reJo.getInt("HexagramPropertyTotalPointLimit");

        JSONObject pJo = reJo.getJSONObject("HexagramProperty");
        doing.Strength = pJo.getInt("Strength");
        doing.Intelligence = pJo.getInt("Intelligence");
        doing.Endurance = pJo.getInt("Endurance");
        doing.Compassion = pJo.getInt("Compassion");
        doing.Sacrifice = pJo.getInt("Sacrifice");

        doing.Requirement = reJo.getString("Requirement");
        doing.VolunteerViewedTime = reJo.getInt("VolunteerViewedTime");
        doing.VolunteerFavoritedTime = reJo.getInt("VolunteerFavoritedTime");
        doing.HasSignedInVolunteerNumber = reJo.getInt("HasSignedInVolunteerNumber");
        doing.hasFavorited = reJo.getBoolean("hasFavorited");
        doing.hasSignined = reJo.getBoolean("hasSignined");
        doing.hasViewed = reJo.getBoolean("hasViewed");

        try {
            doing.VolunteersRecord = readVolunteersRecord(reJo.getJSONArray("VolunteersRecord").getJSONObject(0));
        } catch (Exception e) {
            // Empty
        }

        // Fix
        rightUrlArray(doing.Photos);
        rightUrlArray(doing.Videos);

        return doing;
    }

    private void doRegister(String email, String phone, String password,
            String name, String gender, String role, String referUserId, String validateId,
            String validateCode) throws Exception {
        JSONObject jo = new JSONObject();
        jo.put("email", email);
        jo.put("password", password);
        jo.put("name", name);
        jo.put("role", role);
        jo.put("sex", gender);
        jo.put("phoneNumber", phone);
        jo.put("referralUserId", referUserId);
        jo.put("id", validateId);
        jo.put("validateCode", validateCode);
        VltHttpHelper hh = new VltHttpHelper(mContext);

        String body = hh.postJson(API_HEADER + API_REGISTER, jo);

        checkHttpError(hh, body);

        int responseCode = hh.getResponseCode();
        if (responseCode == 202) {
            // OK
        } else {
            throw new VltException("Register error");
        }
    }

    private String[] doGetHotTags() throws Exception {
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(API_HEADER + API_HOT_TAGS);

        checkHttpError(hh, body);

        JSONArray ja = new JSONArray(body);
        int length = ja.length();
        String[] result = new String[length];
        for (int i = 0; i < length; i++) {
            result[i] = ja.getString(i);
        }
        return result;
    }

    private ValidateImage doGetValidateImage() throws Exception {
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(API_HEADER + API_VALIDATE_IMAGE);

        checkHttpError(hh, body);

        JSONObject jo = new JSONObject(body);
        ValidateImage validateImage = new ValidateImage();
        validateImage.id = jo.getString("id");
        byte[] bytes = Base64.getDecoder().decode(jo.getString("image"));
        validateImage.image = BitmapFactory.decodeByteArray(bytes, 0, bytes.length);
        return validateImage;
    }

    private boolean doIsMyFriend(String id) throws Exception {
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(API_HEADER + API_IS_MY_FRIEND + id);

        checkHttpError(hh, body);

        if (Utils.parseIntSafely(body, 0) == 1) {
            return true;
        } else {
            return false;
        }
    }

    private void doApplyFriend(String id, String comment) throws Exception {
        JSONObject jo = new JSONObject();
        jo.put("id", id);
        jo.put("comment", comment);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.postJson(API_HEADER + API_APPLY_FRIEND, jo);

        checkHttpError(hh, body);

        if (hh.getResponseCode() == 200) {
            // OK
        } else {
            throw new VltException(EXCEPTION_UNAUTHORIZED);
        }
    }

    private void doAddFavorite(String id, boolean favorite) throws Exception {
        JSONObject jo = new JSONObject();
        jo.put("activityId", id);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.postJson(API_HEADER + (favorite ? API_ACTIVITY_FAVORITE : API_ACTIVITY_UNFAVORITE), jo);

        checkHttpError(hh, body);

        if (hh.getResponseCode() == 200) {
            // OK
        } else {
            throw new VltException(EXCEPTION_UNAUTHORIZED);
        }
    }

    private void doRespondFriendApply(String id, boolean isAccept, String comment)
            throws Exception {
        JSONObject jo = new JSONObject();
        jo.put("id", id);
        jo.put("comment", comment);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.postJson(API_HEADER + (isAccept ? API_ACCEPT_FRIEND : API_REFUSE_FRIEND), jo);

        checkHttpError(hh, body);

        if (hh.getResponseCode() == 200) {
            // OK
        } else {
            throw new VltException(EXCEPTION_UNAUTHORIZED);
        }
    }

    private Badge[] doGetUserBadges(String id, int index) throws Exception {
        UrlBuilder ub = new UrlBuilder(API_HEADER + API_USER_BADGES);
        ub.addQuery(API_KEY_ID, id);
        ub.addQuery(API_KEY_SORT_BY_KEY, TextUtils.STRING_EMPTY);
        ub.addQuery(API_KEY_IS_ASCENDING, "false");
        ub.addQuery(API_KEY_PAGE_INDEX, index + 1);
        ub.addQuery(API_KEY_PAGE_SIZE, PAGE_SIZE);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(ub.build());

        JSONArray reJa = new JSONArray(body);
        int length = reJa.length();
        List<Badge> badgeList = new ArrayList<Badge>(length);
        for (int i = 0; i < length; i++) {
            try {
                badgeList.add(readBadge(reJa.getJSONObject(i)));
            } catch (Exception e) {
                // Empty
            }
        }
        return badgeList.toArray(new Badge[badgeList.size()]);
    }

    private void doSignInDoing(String activityId) throws Exception {
        JSONObject jo = new JSONObject();
        jo.put(API_KEY_ID, activityId);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.postJson(API_HEADER + API_SIGN_IN_DOING, jo);

        checkHttpError(hh, body);

        if (hh.getResponseCode() == 202) {
            // OK
        } else {
            throw new VltException(EXCEPTION_UNAUTHORIZED);
        }
    }

    private void doSignOutDoing(String activityId) throws Exception {
        JSONObject jo = new JSONObject();
        jo.put(API_KEY_ID, activityId);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.postJson(API_HEADER + API_SIGN_OUT_DOING, jo);

        checkHttpError(hh, body);

        if (hh.getResponseCode() == 202) {
            // OK
        } else {
            throw new VltException(EXCEPTION_UNAUTHORIZED);
        }
    }

    private Badge doGetBadgeDetail(String id, String name) throws Exception {

        UrlBuilder ub = new UrlBuilder(API_HEADER + API_USER_BADGE_DETAIL);
        ub.addQuery(API_KEY_ID, id);
        ub.addQuery(API_KEY_BADGE_NAME, URLEncoder.encode(name, "utf-8"));

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(ub.build());

        JSONObject reJo = new JSONObject(body);
        Badge badge = new Badge();
        badge.name = reJo.getString("badgeName");
        badge.picture = reJo.getString("badgePicture");
        badge.description = reJo.getString("badgeDescription");
        badge.grantedTime = parserDateSafely1(reJo.getString("badgeGrantedTime"));
        badge.requirementDescription = JSONUtils.getStringArray(reJo, "badgeRequirementDescription");

        // Fix
        badge.picture = rightUrl(badge.picture);

        return badge;
    }

    private void doUploadAvatar(String path) throws Exception {

        String extension = Utils.getExtension(path);
        if (extension == null) {
            extension = "png";
        }

        String mimeType = MimeTypeMap.getSingleton().getMimeTypeFromExtension(extension);
        if (mimeType == null) {
            mimeType = "image/png";
        }

        HttpHelper.FileData fd = new HttpHelper.FileData(new File(path));
        fd.setProperty("Content-Disposition", "form-data; name=\"head_img\"; filename=\"haha." + extension + "\"");
        fd.setProperty("Content-Type", mimeType);

        List<HttpHelper.FormData> datas = new ArrayList<>(1);
        datas.add(fd);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.postFormData(API_HEADER + API_UPLOAD_AVATAR, datas);

        checkHttpError(hh, body);
    }

    private void doUpdateInfo(int type, String newValue) throws Exception {
        String url;
        JSONObject json = new JSONObject();
        switch (type) {
            default:
            case TYPE_EMAIL:
                url = API_UPDATE_EMAIL;
                json.put("email", newValue);
                break;
            case TYPE_PHONE:
                url = API_UPDATE_PHONE;
                json.put("phoneNumber", newValue);
                break;
            case TYPE_AFFILIATION:
                url = API_UPDATE_AFFILIATION;
                json.put("affiliations", newValue);
                break;
            case TYPE_DESCRIPTION:
                url = API_UPDATE_DESCRIPTION;
                json.put("description", newValue);
                break;
        }
        url = API_HEADER + url;

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String result = hh.putJson(url, json);

        checkHttpError(hh, result);
    }

    private Comment[] doGetComments(String id, String type, int index) throws Exception {
        UrlBuilder ub = new UrlBuilder(API_HEADER + API_COMMENT);
        ub.addQuery(API_KEY_ID, id);
        ub.addQuery(API_KEY_TYPE, type);
        ub.addQuery(API_KEY_SORT_BY_KEY, TextUtils.STRING_EMPTY);
        ub.addQuery(API_KEY_IS_ASCENDING, "false");
        ub.addQuery(API_KEY_PAGE_INDEX, index + 1);
        ub.addQuery(API_KEY_PAGE_SIZE, PAGE_SIZE);

        VltHttpHelper vhh = new VltHttpHelper(mContext);
        String body = vhh.get(ub.build());

        checkHttpError(vhh, body);

        Comment[] comments = ArrayUtils.list2Array(JSON.parseArray(body, Comment.class), Comment.class);
        if (comments == null) {
            throw new VltException("Can't get comments");
        }

        int length = comments.length;
        for (int i = 0; i < length; i++) {
            comments[i].Avatar = rightUrl(comments[i].Avatar);
        }
        return comments;
    }



    private void doPostComment(String userId, boolean isCommentOnComment,
            String fatherCommentId, String content) throws Exception {
        JSONObject jo = new JSONObject();
        jo.put("id", userId);
        jo.put("isCommentOnComment", isCommentOnComment);
        jo.put("fatherCommentId", fatherCommentId);
        jo.put("content", content);

        VltHttpHelper vhh = new VltHttpHelper(mContext);
        String body = vhh.postJson(API_HEADER + API_COMMENT, jo);

        checkHttpError(vhh, body);
    }

    private Summary doGetSummary(String activityId) throws Exception {
        UrlBuilder ub = new UrlBuilder(API_HEADER + API_SUMMARY);
        ub.addQuery("id", activityId);

        VltHttpHelper vhh = new VltHttpHelper(mContext);
        String body = vhh.get(ub.build());

        checkHttpError(vhh, body);
        JSONObject jo = new JSONObject(body);
        Summary summary = new Summary();
        summary.content = jo.getString("Content");

        return summary;
    }

    private int doGetRate(String activityId) throws Exception {
        UrlBuilder ub = new UrlBuilder(API_HEADER + API_RATE);
        ub.addQuery("id", activityId);

        VltHttpHelper vhh = new VltHttpHelper(mContext);
        String body = vhh.get(ub.build());

        checkHttpError(vhh, body);

        return Utils.parseIntSafely(body, 0);
    }

    private void doRate(String activityId, int rate) throws Exception {
        JSONObject jo = new JSONObject();
        jo.put("activityId", activityId);
        jo.put("rate", rate);

        VltHttpHelper vhh = new VltHttpHelper(mContext);
        String body = vhh.postJson(API_HEADER + API_RATE, jo);

        checkHttpError(vhh, body);
    }

    private void doBgJob(BgJobHelper bjh) {
        Utils.execute(false, new AsyncTask<Object, Void, Object[]>() {
            @Override
            protected Object[] doInBackground(Object... params) {
                BgJobHelper bjh = (BgJobHelper) params[0];
                return new Object[] {bjh.doInBackground(), bjh};
            }

            @Override
            protected void onPostExecute(Object[] resultPackage) {
                Object result = resultPackage[0];
                BgJobHelper bjh = (BgJobHelper) resultPackage[1];
                bjh.onPostExecute(result);
            }
        }, bjh);
    }

    private static interface BgJobHelper {
        public Object doInBackground();

        public void onPostExecute(Object result);
    }

    public static interface OnCheckUpdateListener {
        public void onSuccess(CheckUpdateInfo cui);

        public void onFailure(Exception e);
    }

    public static interface OnLoginListener {
        public void onSuccess(VltAccount account);

        public void onFailure(Exception e);
    }

    public interface OnGetUserListener {
        public void onSuccess(User user);

        public void onFailure(Exception e);
    }

    public interface OnGetVolunteerListener {
        public void onSuccess(Volunteer user);

        public void onFailure(Exception e);
    }

    public interface OnGetUserWithInfoListener {
        public void onSuccess(User user);

        public void onFailure(Exception e);
    }

    public interface OnLoginWithInfoListener {
        public void onSuccess(VltAccount account);

        public void onFailure(Exception e);
    }

    public interface  OnGetFeedsListener {
        public void onSuccess(Feed[] feeds);

        public void onFailure(Exception e);
    }

    public interface  OnGetMyFriendsListener {
        public void onSuccess(Friend[] friends);

        public void onFailure(Exception e);
    }

    public interface  OnGetRecommendFriendsListener {
        public void onSuccess(Friend[] friends);

        public void onFailure(Exception e);
    }

    public interface  OnSearchNotMyFriendListener {
        public void onSuccess(Friend[] friends);

        public void onFailure(Exception e);
    }

    public interface OnVolunteerActionListener {
        public void onSuccess();

        public void onFailure(Exception e);
    }

    public interface OnGetHotTagsListener {
        public void onSuccess(String[] tags);

        public void onFailure(Exception e);
    }

    public interface OnMyNearbyFriendsRankListener {
        /**
         * @param friends Friend array
         * @param startRank The first friend rank in array
         * @param MyPosition My position in array
         */
        public void onSuccess(Friend[] friends, int startRank, int MyPosition);

        public void onFailure(Exception e);
    }

    public interface OnFriendApplyListener {
        public void onSuccess(ApplyFromMe[] applyFromMes, ApplyToMe[] applyToMes);

        public void onFailure(Exception e);
    }

    public interface OnGetFavoriteDoingsListener {
        public void onSuccess(Doing[] doings);

        public void onFailure(Exception e);
    }

    public interface OnDoingsListener {
        public void onSuccess(Doing[] doings);

        public void onFailure(Exception e);
    }

    public interface OnMyDoingsListener {
        public void onSuccess(Doing[] doings);

        public void onFailure(Exception e);
    }

    public interface  OnDoingListener {
        public void onSuccess(Doing doing);

        public void onFailure(Exception e);
    }

    public interface OnGetValidateImageListener {
        public void onSuccess(ValidateImage validateImage);

        public void onFailure(Exception e);
    }

    public interface OnRegisterListener {
        public void onSuccess();

        public void onFailure(Exception e);
    }

    public interface OnIsMyFriendHelperListener {
        public void onSuccess(boolean isMyFriend);

        public void onFailure(Exception e);
    }

    public interface OnApplyFriendListener {
        public void onSuccess();

        public void onFailure(Exception e);
    }

    public interface OnAddFavoriteListener {
        public void onSuccess();

        public void onFailure(Exception e);
    }

    public interface OnRespondFriendApplyListener {
        public void onSuccess();

        public void onFailure(Exception e);
    }

    public interface OnGetUserBadgesListener {
        public void onSuccess(Badge[] badges);

        public void onFailure(Exception e);
    }

    public interface OnGetBadgeDetailListener {
        public void onSuccess(Badge badge);

        public void onFailure(Exception e);
    }

    public interface OnSignInDoingListener {
        public void onSuccess();

        public void onFailure(Exception e);
    }

    public interface OnSignOutDoingListener {
        public void onSuccess();

        public void onFailure(Exception e);
    }

    public interface OnUploadAvatarListener {
        public void onSuccess();

        public void onFailure(Exception e);
    }

    public interface OnUpdateInfoListener {
        public void onSuccess();

        public void onFailure(Exception e);
    }

    public interface OnGetCommentsListener {
        void onSuccess(Comment[] comments);

        void onFailure(Exception e);
    }

    public interface OnPostCommentListener {
        void onSuccess();

        void onFailure(Exception e);
    }

    public interface OnGetSummaryListener {
        void onSuccess(Summary summary);

        void onFailure(Exception e);
    }

    public interface OnGetRateListener {
        void onSuccess(int rate);

        void onFailure(Exception e);
    }

    public interface OnRateListener {
        void onSuccess();

        void onFailure(Exception e);
    }

    private class CheckUpdateHelper implements BgJobHelper {
        private int mCurrentVersion;
        private OnCheckUpdateListener mListener;


        public CheckUpdateHelper(int currentVersion, OnCheckUpdateListener listener) {
            mCurrentVersion = currentVersion;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                return doCheckUpdate(mCurrentVersion);
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof CheckUpdateInfo) {
                    mListener.onSuccess((CheckUpdateInfo) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class LoginHelper implements BgJobHelper {
        private String mEmail;
        private String mPassword;
        private OnLoginListener mListener;

        public LoginHelper(String email, String password, OnLoginListener l) {
            mEmail = email;
            mPassword = password;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doLogin(mEmail, mPassword);
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof VltAccount) {
                    mListener.onSuccess((VltAccount) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetUserHelper implements BgJobHelper {
        private String mId;
        private OnGetUserListener mListener;

        public GetUserHelper(String id, OnGetUserListener l) {
            mId = id;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetUser(mId);
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof User) {
                    mListener.onSuccess((User) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetVolunteerHelper implements BgJobHelper {
        private User mUser;
        private String mId;
        private OnGetVolunteerListener mListener;

        public GetVolunteerHelper(User user, String id, OnGetVolunteerListener l) {
            mUser = user;
            mId = id;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetVolunteer(mUser, mId);
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Volunteer) {
                    mListener.onSuccess((Volunteer) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetUserWithInfoHelper implements BgJobHelper {
        private String mId;
        private OnGetUserWithInfoListener mListener;

        public GetUserWithInfoHelper(String id, OnGetUserWithInfoListener l) {
            mId = id;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                User user = doGetUser(mId);
                switch (user.role) {
                    case VltUtils.ROLE_VOLUNTEER:
                        user = doGetVolunteer(user, mId);
                        break;
                    case VltUtils.ROLE_ORGANIZER:
                        // TODO
                        break;
                    case VltUtils.ROLE_ORGANIZATION:
                        // TODO
                        break;
                    default:
                        // TODO
                        break;
                }
                return user;
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof User) {
                    mListener.onSuccess((User) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class LoginWithInfoHelper implements BgJobHelper {
        private String mEmail;
        private String mPassword;
        private OnLoginWithInfoListener mListener;

        public LoginWithInfoHelper(String email, String password, OnLoginWithInfoListener l) {
            mEmail = email;
            mPassword = password;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                VltAccount account = doLogin(mEmail, mPassword);
                // I need to put account into store for cookie
                VltAccountStore.getInstance(mContext).addVltAccount(account);
                User user = doGetUser(account.userId);
                switch (user.role) {
                    case VltUtils.ROLE_VOLUNTEER:
                        account.user = doGetVolunteer(user, account.userId);
                        break;
                    case VltUtils.ROLE_ORGANIZER:
                        // TODO
                        break;
                    case VltUtils.ROLE_ORGANIZATION:
                        // TODO
                        break;
                    default:
                        // TODO
                        break;
                }
                return account;
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof VltAccount) {
                    mListener.onSuccess((VltAccount) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetFeedsHelper implements BgJobHelper {
        private String mId;
        private int mIndex;
        private OnGetFeedsListener mListener;

        public GetFeedsHelper(String id, int index, OnGetFeedsListener l) {
            mId = id;
            mIndex = index;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetFeeds(mId, mIndex);
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Feed[]) {
                    mListener.onSuccess((Feed[]) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetMyFriendsHelper implements BgJobHelper {
        private OnGetMyFriendsListener mListener;

        public GetMyFriendsHelper(OnGetMyFriendsListener l) {
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetMyFriends();
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Friend[]) {
                    mListener.onSuccess((Friend[]) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetRecommendFriendsHelper implements BgJobHelper {
        private OnGetRecommendFriendsListener mListener;

        public GetRecommendFriendsHelper(OnGetRecommendFriendsListener l) {
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetRecommendFriends();
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Friend[]) {
                    mListener.onSuccess((Friend[]) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }



    private class SearchNotMyFriendHelper implements BgJobHelper {
        private String mName;
        private OnSearchNotMyFriendListener mListener;

        public SearchNotMyFriendHelper(String name, OnSearchNotMyFriendListener l) {
            mName = name;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doSearchNotMyFriend(mName);
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Friend[]) {
                    mListener.onSuccess((Friend[]) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class VolunteerAction implements BgJobHelper {
        private String mId;
        private OnVolunteerActionListener mListener;

        public VolunteerAction(String id, OnVolunteerActionListener l) {
            mId = id;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                doVolunteerAction(mId);
                return RETURE_OBJECT;
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof ReturnObject) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class MyNearbyFriendsRank implements BgJobHelper {
        private String mKey;
        private OnMyNearbyFriendsRankListener mListener;

        private Object mResult;
        private int mStartRank;
        private int mMyPosition;

        public MyNearbyFriendsRank (String key, OnMyNearbyFriendsRankListener l) {
            mKey = key;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                int myRank = doMyRank(mKey);
                int targetPageIndex = MathUtils.div(myRank, PAGE_SIZE);
                mStartRank = (targetPageIndex - 1) * PAGE_SIZE + 1;
                mMyPosition = myRank - mStartRank;
                mResult = doMyFriendsRank(mKey, targetPageIndex);
            } catch (Exception e) {
                mResult = e;
            }
            return null;
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (mResult instanceof Friend[]) {
                    mListener.onSuccess((Friend[]) mResult, mStartRank, mMyPosition);
                } else if (mResult instanceof Exception) {
                    mListener.onFailure((Exception) mResult);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class FriendApply implements BgJobHelper {
        private OnFriendApplyListener mListener;

        private Exception mException;
        private ApplyFromMe[] mApplyFromMeArray;
        private ApplyToMe[] mApplyToMeArray;

        public FriendApply(OnFriendApplyListener l) {
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                mApplyFromMeArray = doApplyFromMe();
                mApplyToMeArray = doApplyToMe();
            }catch (Exception e) {
                mException = e;
            }
            return null;
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (mException != null) {
                    mListener.onFailure(mException);
                } else {
                    mListener.onSuccess(mApplyFromMeArray, mApplyToMeArray);
                }
            }
        }
    }

    private class Doings implements BgJobHelper {
        private String mFiltersource;
        private String mStage;
        private int mPageindex;
        private OnDoingsListener mListener;

        public Doings(String filtersource, String stage, int pageindex, OnDoingsListener l) {
            mFiltersource = filtersource;
            mStage = stage;
            mPageindex = pageindex;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doDoings(mFiltersource, mStage, mPageindex);
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Doing[]) {
                    mListener.onSuccess((Doing[]) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetFavoriteDoingsHelper implements BgJobHelper {
        private String mFiltersource;
        private String mStage;
        private int mPageindex;
        private OnGetFavoriteDoingsListener mListener;

        public GetFavoriteDoingsHelper(String filtersource, String stage, int pageindex, OnGetFavoriteDoingsListener l) {
            mFiltersource = filtersource;
            mStage = stage;
            mPageindex = pageindex;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetFavoriteDoings(mFiltersource, mStage, mPageindex);
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Doing[]) {
                    mListener.onSuccess((Doing[]) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class MyDoings implements BgJobHelper {
        private String mId;
        private String mFiltersource;
        private String mStage;
        private int mPageindex;
        private OnMyDoingsListener mListener;

        public MyDoings(String id, String filtersource, String stage, int pageindex, OnMyDoingsListener l) {
            mId = id;
            mFiltersource = filtersource;
            mStage = stage;
            mPageindex = pageindex;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doMyDoings(mId, mFiltersource, mStage, mPageindex);
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Doing[]) {
                    mListener.onSuccess((Doing[]) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetDoing implements BgJobHelper {
        private String mId;
        private OnDoingListener mListener;

        public GetDoing(String id, OnDoingListener l) {
            mId = id;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doDoing(mId);
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Doing) {
                    mListener.onSuccess((Doing) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetHotTagsHelper implements BgJobHelper {
        private OnGetHotTagsListener mListener;

        public GetHotTagsHelper(OnGetHotTagsListener l) {
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetHotTags();
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof String[]) {
                    mListener.onSuccess((String[]) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetValidateImageHelper implements BgJobHelper {
        private OnGetValidateImageListener mListener;

        public GetValidateImageHelper(OnGetValidateImageListener l) {
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetValidateImage();
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof ValidateImage) {
                    mListener.onSuccess((ValidateImage) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class RegisterHelper implements BgJobHelper {
        private String mEmail;
        private String mPhone;
        private String mPassword;
        private String mName;
        private String mGender;
        private String mRole;
        private String mReferUserId;
        private String mValidateId;
        private String mValidateCode;
        private OnRegisterListener mListener;

        public RegisterHelper(String email, String phone, String password,
                String name, String gender, String role, String referUserId,
                String validateId, String validateCode, OnRegisterListener l) {
            mEmail = email;
            mPhone = phone;
            mPassword = password;
            mName = name;
            mGender = gender;
            mRole = role;
            mReferUserId = referUserId;
            mValidateId = validateId;
            mValidateCode = validateCode;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                doRegister(mEmail, mPhone, mPassword, mName, mGender, mRole, mReferUserId,
                        mValidateId, mValidateCode);
                return RETURE_OBJECT;
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result == RETURE_OBJECT) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class IsMyFriendHelper implements BgJobHelper {
        private String mId;
        private OnIsMyFriendHelperListener mListener;

        public IsMyFriendHelper(String id, OnIsMyFriendHelperListener listener) {
            mId = id;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                return doIsMyFriend(mId);
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Boolean) {
                    mListener.onSuccess((Boolean) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class ApplyFriendHelper implements BgJobHelper {
        private String mId;
        private String mComment;
        private OnApplyFriendListener mListener;

        public ApplyFriendHelper(String id, String comment, OnApplyFriendListener listener) {
            mId = id;
            mComment = comment;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                doApplyFriend(mId, mComment);
                return RETURE_OBJECT;
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result == RETURE_OBJECT) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class AddFavoriteHelper implements BgJobHelper {
        private String mId;
        private boolean mFavorite;
        private OnAddFavoriteListener mListener;

        public AddFavoriteHelper(String id, boolean favorite, OnAddFavoriteListener listener) {
            mId = id;
            mFavorite = favorite;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                doAddFavorite(mId, mFavorite);
                return RETURE_OBJECT;
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result == RETURE_OBJECT) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class RespondFriendApplyHelper implements BgJobHelper {
        private String mId;
        private boolean mIsAccpet;
        private String mComment;
        private OnRespondFriendApplyListener mListener;

        public RespondFriendApplyHelper(String id, boolean isAccpet, String comment,
                OnRespondFriendApplyListener l) {
            mId = id;
            mIsAccpet = isAccpet;
            mComment = comment;
            mListener = l;
        }

        @Override
        public Object doInBackground() {
            try {
                doRespondFriendApply(mId, mIsAccpet, mComment);
                return RETURE_OBJECT;
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result == RETURE_OBJECT) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetUserBadgesHelper implements BgJobHelper {

        private String mId;
        private int mIndex;
        private OnGetUserBadgesListener mListener;

        public GetUserBadgesHelper(String id, int index, OnGetUserBadgesListener listener) {
            mId = id;
            mIndex = index;
            mListener = listener;
        }


        @Override
        public Object doInBackground() {
            try {
                return doGetUserBadges(mId, mIndex);
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Badge[]) {
                    mListener.onSuccess((Badge[]) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetBadgeDetailHelper implements BgJobHelper {
        private String mId;
        private String mName;
        private OnGetBadgeDetailListener mListener;

        public GetBadgeDetailHelper(String id, String name, OnGetBadgeDetailListener listener) {
            mId = id;
            mName = name;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetBadgeDetail(mId, mName);
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Badge) {
                    mListener.onSuccess((Badge) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }




    private class SiginInDoingHelper implements BgJobHelper {

        private String mId;
        private OnSignInDoingListener mListener;

        public SiginInDoingHelper(String id, OnSignInDoingListener listener) {
            mId = id;
            mListener = listener;
        }


        @Override
        public Object doInBackground() {
            try {
                doSignInDoing(mId);
                return RETURE_OBJECT;
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result == RETURE_OBJECT) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class SiginOutDoingHelper implements BgJobHelper {

        private String mId;
        private OnSignOutDoingListener mListener;

        public SiginOutDoingHelper(String id, OnSignOutDoingListener listener) {
            mId = id;
            mListener = listener;
        }


        @Override
        public Object doInBackground() {
            try {
                doSignOutDoing(mId);
                return RETURE_OBJECT;
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result == RETURE_OBJECT) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class UploadAvatarHelper implements BgJobHelper {

        private String mPath;
        private OnUploadAvatarListener mListener;

        public UploadAvatarHelper(String path, OnUploadAvatarListener listener) {
            mPath = path;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                doUploadAvatar(mPath);
                return RETURE_OBJECT;
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result == RETURE_OBJECT) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class UpdateInfoHelper implements BgJobHelper {
        private int mType;
        private String mNewValue;
        private OnUpdateInfoListener mListener;

        public UpdateInfoHelper(int type, String newValue, OnUpdateInfoListener listener) {
            mType = type;
            mNewValue = newValue;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                doUpdateInfo(mType, mNewValue);
                return RETURE_OBJECT;
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result == RETURE_OBJECT) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetCommentsHelper implements BgJobHelper {

        private String mId;
        private String mType;
        private int mIndex;
        private OnGetCommentsListener mListener;

        public GetCommentsHelper(String id, String type, int index, OnGetCommentsListener listener) {
            mId = id;
            mType = type;
            mIndex = index;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetComments(mId, mType, mIndex);
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Comment[]) {
                    mListener.onSuccess((Comment[]) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class PostCommentHelper implements BgJobHelper {

        private String mUserId;
        private boolean mIsCommentOnComment;
        private String mFatherCommentId;
        private String mComment;
        private OnPostCommentListener mListener;

        public PostCommentHelper(String userId, boolean isCommentOnComment,
                String fatherCommentId, String content, OnPostCommentListener listener) {
            mUserId = userId;
            mIsCommentOnComment = isCommentOnComment;
            mFatherCommentId = fatherCommentId;
            mComment = content;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                doPostComment(mUserId, mIsCommentOnComment, mFatherCommentId, mComment);
                return RETURE_OBJECT;
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result == RETURE_OBJECT) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }

    private class GetSummaryHelper implements BgJobHelper {

        private String mActivityId;
        private OnGetSummaryListener mListener;

        public GetSummaryHelper(String activityId, OnGetSummaryListener listener) {
            mActivityId = activityId;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetSummary(mActivityId);
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Summary) {
                    mListener.onSuccess((Summary) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }



    private class GetRateHelper implements BgJobHelper {

        private String mActivityId;
        private OnGetRateListener mListener;

        public GetRateHelper(String activityId, OnGetRateListener listener) {
            mActivityId = activityId;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                return doGetRate(mActivityId);
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result instanceof Integer) {
                    mListener.onSuccess((Integer) result);
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }


    private class RateHelper implements BgJobHelper {

        private String mActivityId;
        private int mRate;
        private OnRateListener mListener;

        public RateHelper(String activityId, int rate, OnRateListener listener) {
            mActivityId = activityId;
            mRate = rate;
            mListener = listener;
        }

        @Override
        public Object doInBackground() {
            try {
                doRate(mActivityId, mRate);
                return RETURE_OBJECT;
            }catch (Exception e) {
                return e;
            }
        }

        @Override
        public void onPostExecute(Object result) {
            if (mListener != null) {
                if (result == RETURE_OBJECT) {
                    mListener.onSuccess();
                } else if (result instanceof Exception) {
                    mListener.onFailure((Exception) result);
                } else {
                    mListener.onFailure(new IllegalStateException(EXCEPTION_ERROR_RESULT));
                }
            }
        }
    }



    public void checkUpdate(int currentVersion, OnCheckUpdateListener l) {
        doBgJob(new CheckUpdateHelper(currentVersion, l));
    }

    public void login(String email, String password, OnLoginListener l) {
        doBgJob(new LoginHelper(email, password, l));
    }

    public void getUser(String id, OnGetUserListener l) {
        doBgJob(new GetUserHelper(id, l));
    }

    public void getVolunteer(User user, String id, OnGetVolunteerListener l) {
        doBgJob(new GetVolunteerHelper(user, id, l));
    }

    public void getUserWithInfo(String id, OnGetUserWithInfoListener l) {
        doBgJob(new GetUserWithInfoHelper(id, l));
    }

    public void loginWithInfo(String email, String password, OnLoginWithInfoListener l) {
        doBgJob(new LoginWithInfoHelper(email, password, l));
    }

    public void getFeeds(String id, int index, OnGetFeedsListener l) {
        doBgJob(new GetFeedsHelper(id, index, l));
    }

    public void getMyFriends(OnGetMyFriendsListener l) {
        doBgJob(new GetMyFriendsHelper(l));
    }

    public void getRecommendFriends(OnGetRecommendFriendsListener l) {
        doBgJob(new GetRecommendFriendsHelper(l));
    }

    public void getSearchNotMyFriend(String name, OnSearchNotMyFriendListener l) {
        doBgJob(new SearchNotMyFriendHelper(name, l));
    }

    public void volunteerAction(String id, OnVolunteerActionListener l) {
        doBgJob(new VolunteerAction(id, l));
    }

    public void myNearbyFriendsRank(String key, OnMyNearbyFriendsRankListener l) {
        doBgJob(new MyNearbyFriendsRank(key, l));
    }

    public void friendApply(OnFriendApplyListener l) {
        doBgJob(new FriendApply(l));
    }

    public void doings(String filtersource, String stage, int pageindex, OnDoingsListener l) {
        doBgJob(new Doings(filtersource, stage, pageindex, l));
    }

    public void getFavoriteDoings(String filtersource, String stage, int pageindex, OnGetFavoriteDoingsListener l) {
        doBgJob(new GetFavoriteDoingsHelper(filtersource, stage, pageindex, l));
    }

    public void myDoings(String id, String filtersource, String stage, int pageindex, OnMyDoingsListener l) {
        doBgJob(new MyDoings(id, filtersource, stage, pageindex, l));
    }

    public void getDoing(String id, OnDoingListener l) {
        doBgJob(new GetDoing(id, l));
    }

    public void getHotTags(OnGetHotTagsListener l) {
        doBgJob(new GetHotTagsHelper(l));
    }

    public void getValidateImage(OnGetValidateImageListener l) {
        doBgJob(new GetValidateImageHelper(l));
    }

    public void register(String email, String phone, String password,
            String name,String gender, String role, String referUserId,
            String validateId, String validateCode, OnRegisterListener l) {
        doBgJob(new RegisterHelper(email, phone, password, name, gender, role,
                referUserId, validateId, validateCode, l));
    }

    public void isMyFriend(String id, OnIsMyFriendHelperListener l) {
        doBgJob(new IsMyFriendHelper(id, l));
    }

    public void applyFriend(String id, String comment, OnApplyFriendListener l) {
        doBgJob(new ApplyFriendHelper(id, comment, l));
    }

    public void addFavorite(String id, boolean favorite, OnAddFavoriteListener l) {
        doBgJob(new AddFavoriteHelper(id, favorite, l));
    }

    public void respondFriendApply(String id, boolean isAccpet, String comment,
            OnRespondFriendApplyListener l) {
        doBgJob(new RespondFriendApplyHelper(id, isAccpet, comment, l));
    }

    public void getUserBadges(String id, int index, OnGetUserBadgesListener l) {
        doBgJob(new GetUserBadgesHelper(id, index, l));
    }

    public void getBadgeDetail(String id, String name, OnGetBadgeDetailListener l) {
        doBgJob(new GetBadgeDetailHelper(id, name, l));
    }

    public void signInDoing(String id, OnSignInDoingListener l) {
        doBgJob(new SiginInDoingHelper(id, l));
    }

    public void signOutDoing(String id, OnSignOutDoingListener l) {
        doBgJob(new SiginOutDoingHelper(id, l));
    }

    public void uploadAvatar(String path, OnUploadAvatarListener l) {
        doBgJob(new UploadAvatarHelper(path, l));
    }

    public void updateInfo(int type, String newValue, OnUpdateInfoListener l) {
        doBgJob(new UpdateInfoHelper(type, newValue, l));
    }

    public void getComments(String id, String type, int index, OnGetCommentsListener l) {
        doBgJob(new GetCommentsHelper(id, type, index, l));
    }

    public void postComments(String userId, boolean isCommentOnComment,
            String fatherCommentId, String content, OnPostCommentListener listener) {
        doBgJob(new PostCommentHelper(userId, isCommentOnComment, fatherCommentId,
                content, listener));
    }

    public void getSummary(String activityId, OnGetSummaryListener listener) {
        doBgJob(new GetSummaryHelper(activityId, listener));
    }

    public void getRate(String activityId, OnGetRateListener listener) {
        doBgJob(new GetRateHelper(activityId, listener));
    }

    public void rate(String activityId, int rate, OnRateListener listener) {
        doBgJob(new RateHelper(activityId, rate, listener));
    }

    public class VltException extends Exception {
        private static final long serialVersionUID = 1L;

        public VltException() {
            super("VltException");
        }

        public VltException(String detailMessage) {
            super(detailMessage);
        }
    }
}
