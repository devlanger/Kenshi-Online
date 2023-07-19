import React, { Component } from 'react';
import {MailForm} from "./Form";
import {YouTubeVideo} from "./YoutubeVideo";

export class Home extends Component {
  static displayName = Home.name;
  static videoId = 'lucfAWJiCBc';

  render() {
    return (
      <div>
        <h1>Welcome to Shindo Online!</h1>
          <YouTubeVideo videoId={Home.videoId}/>
        <p>Enter your e-mail below to sign-up for the closed beta.</p>
          <MailForm></MailForm>
          <p>*There is no guarantee that the access will be granted to the closed beta.</p>
          <p>We are going to select participants randomly.</p>
      </div>
    );
  }
}
