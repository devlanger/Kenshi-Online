import React, { Component } from 'react';
import {Button, Container, Form, Input, Label} from 'reactstrap';
import { NavMenu } from './NavMenu';

export class MailForm extends Component {
    static displayName = MailForm.name;

    render() {
        return (
            <div>
                <Form>
                    <Label>E-Mail</Label>
                    <Input/>
                    <Button variant="primary" type="submit">
                        Submit
                    </Button>
                </Form>
            </div>
        );
    }
}
