package org.volq.volunteer.cardview;

import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import org.volq.volunteer.R;

public class CommentHolder extends RecyclerView.ViewHolder {

    public ImageView avatar;
    public TextView name;
    public TextView date;
    public TextView text;

    public CommentHolder(View itemView) {
        super(itemView);

        avatar = (ImageView) itemView.findViewById(R.id.avatar);
        name = (TextView) itemView.findViewById(R.id.name);
        date = (TextView) itemView.findViewById(R.id.date);
        text = (TextView) itemView.findViewById(R.id.text);
    }

    public static CommentHolder createViewHolder(LayoutInflater inflater, ViewGroup parent) {
        return new CommentHolder(inflater.inflate(R.layout.comment_item, parent, false));
    }

}
