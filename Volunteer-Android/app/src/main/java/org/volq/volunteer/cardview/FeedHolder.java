package org.volq.volunteer.cardview;

import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import org.volq.volunteer.R;

import de.hdodenhof.circleimageview.CircleImageView;

public class FeedHolder extends RecyclerView.ViewHolder {

    public CircleImageView avatar;
    public TextView owner;
    public TextView subtitle;
    public TextView message;
    public ImageView thumb;

    public FeedHolder(final View itemView) {
        super(itemView);

        avatar = (CircleImageView) itemView.findViewById(R.id.avatar);
        owner = (TextView) itemView.findViewById(R.id.owner);
        subtitle = (TextView) itemView.findViewById(R.id.subtitle);
        message = (TextView) itemView.findViewById(R.id.message);
        thumb = (ImageView) itemView.findViewById(R.id.thumb);
    }

    public static FeedHolder createViewHolder(LayoutInflater inflater, ViewGroup parent) {
        return new FeedHolder(inflater.inflate(R.layout.simple_card_view, parent, false));
    }
}
