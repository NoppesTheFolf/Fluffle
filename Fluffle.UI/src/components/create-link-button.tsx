import * as React from 'react'
import urlcat from 'urlcat'
import Api from '../services/api'
import Icon from './icon'

const CreateLinkButton = ({ data }) => {
    const states = {
        IDLE: 'IDLE',
        WAITING: 'WAITING',
        SUCCESS: 'SUCCESS',
        ERROR: 'ERROR'
    }
    const [state, setState] = React.useState(states.IDLE);
    const [url, setUrl] = React.useState<string>();
    const [count, setCount] = React.useState(-1);

    function copyToClipboard(url: string) {
        navigator.clipboard.writeText(url);
        setCount(count + 1);
    }

    function createLink() {
        if (new URLSearchParams(window.location.hash).has("#url")) {
                copyToClipboard(window.location.href);
                setState(states.SUCCESS);
            return;
        }

        if (state === states.SUCCESS) {
            copyToClipboard(url!);
            return;
        }

        if (state !== states.IDLE) {
            return;
        }

        setState(states.WAITING);
        fetch(data.parameters.imageUrl).then(r => r.blob()).then(file => {
            Api.createLink(file).then(data => {
                const url = urlcat(process.env.GATSBY_SITE_URL as string, 'q/:id', { id: data.id });
                setUrl(url);
                copyToClipboard(url);
                setState(states.SUCCESS);
            }).catch(_ => {
                setState(states.ERROR);
            });
        });
    }

    const presentations = {
        WAITING: ['Creating link...', 'fa-circle-notch', true, 'secondary'],
        SUCCESS: [`Link copied to clipboard! ${count > 0 ? `(${count})` : ''}`, 'copy', false, 'success'],
        ERROR: ['Something went wrong', 'report-problem', false, 'danger'],
        IDLE: ['Share results', 'link', false, 'secondary']
    };
    const [text, iconName, animate, color] = presentations[String(state)];

    return (
        <div className="flex flex-col lg:flex-row lg:items-center lg:space-x-2 lg:mt-4">
            <button className={`btn btn-sm btn-${color} space-x-1`} onClick={createLink}>
                <Icon className={animate ? "spin" : undefined} name={iconName} />
                <span>{text}</span>
            </button>
            <a className="text-sm text-center link-muted italic" href={url} target="_blank">
                {url}
            </a>
        </div>
    )
}

export default CreateLinkButton
