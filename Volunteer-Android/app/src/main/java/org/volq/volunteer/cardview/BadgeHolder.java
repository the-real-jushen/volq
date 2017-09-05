package org.volq.volunteer.cardview;

import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import com.hippo.widget.FixedAspectImageView;

import org.volq.volunteer.R;

public class BadgeHolder extends RecyclerView.ViewHolder {

    public TextView name;
    public FixedAspectImageView picture;

    public BadgeHolder(View itemView) {
        super(itemView);

        name = (TextView) itemView.findViewById(R.id.badge_name);
        picture = (FixedAspectImageView) itemView.findViewById(R.id.badge_picture);
    }

    public static BadgeHolder createViewHolder(LayoutInflater inflater, ViewGroup parent) {
        return new BadgeHolder(inflater.inflate(R.layout.badge_item, parent, false));
    }
}
