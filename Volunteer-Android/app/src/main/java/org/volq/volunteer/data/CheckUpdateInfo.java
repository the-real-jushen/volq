package org.volq.volunteer.data;

public class CheckUpdateInfo extends FastJSONObject {

    private Boolean isLatest;
    private String changelog;
    private String downloadAppLink;

    public Boolean getIsLatest() {
        return isLatest;
    }

    public void setIsLatest(Boolean isLatest) {
        this.isLatest = isLatest;
    }

    public String getChangelog() {
        return changelog;
    }

    public void setChangelog(String changelog) {
        this.changelog = changelog;
    }

    public String getDownloadAppLink() {
        return downloadAppLink;
    }

    public void setDownloadAppLink(String downloadAppLink) {
        this.downloadAppLink = downloadAppLink;
    }
}
