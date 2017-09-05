package org.volq.volunteer.cardview;

import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import org.volq.volunteer.R;

public class DoingHolder extends RecyclerView.ViewHolder {

    public ImageView thumb;
    public TextView title;
    public TextView state;
    public TextView time;
    public TextView browse;
    public TextView favorite;
    public TextView join;

    public DoingHolder(View itemView) {
        super(itemView);

        thumb = (ImageView) itemView.findViewById(R.id.thumb);
        title = (TextView) itemView.findViewById(R.id.title);
        state = (TextView) itemView.findViewById(R.id.state);
        time = (TextView) itemView.findViewById(R.id.time);
        browse = (TextView) itemView.findViewById(R.id.browse);
        favorite = (TextView) itemView.findViewById(R.id.favorite);
        join = (TextView) itemView.findViewById(R.id.join);
    }

    public static DoingHolder createViewHolder(LayoutInflater inflater, ViewGroup parent) {
        return new DoingHolder(inflater.inflate(R.layout.doing_card_view, parent, false));
    }

}
