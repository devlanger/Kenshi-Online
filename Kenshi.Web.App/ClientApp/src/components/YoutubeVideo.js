import React from 'react';
import YouTube from 'react-youtube';

export function YouTubeVideo({ videoId }) {
    const opts = {
        height: '315',
        width: '560',
        playerVars: {
            // You can add additional parameters here (e.g., autoplay, controls, etc.)
        }
    };

    return <YouTube videoId={videoId} opts={opts} />;
}