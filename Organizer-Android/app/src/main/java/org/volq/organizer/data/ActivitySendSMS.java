package org.volq.organizer.data;

import java.util.Map;

public class ActivitySendSMS {

    private String activityId;
    private String content;
    private Map<String, String> volunteerNameAndPhoneNumber;

    public String getActivityId() {
        return activityId;
    }

    public void setActivityId(String activityId) {
        this.activityId = activityId;
    }

    public String getContent() {
        return content;
    }

    public void setContent(String content) {
        this.content = content;
    }

    public Map<String, String> getVolunteerNameAndPhoneNumber() {
        return volunteerNameAndPhoneNumber;
    }

    public void setVolunteerNameAndPhoneNumber(Map<String, String> volunteerNameAndPhoneNumber) {
        this.volunteerNameAndPhoneNumber = volunteerNameAndPhoneNumber;
    }
}
