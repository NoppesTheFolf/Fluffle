import classNames from 'classnames'
import * as React from 'react'
import { progressBar, progressBarStriped, progressBarAnimated } from './progress-bar-part.module.scss'

interface ProgressBarProps {
    color: string;
    percentage: number;
    isStriped: boolean;
    isAnimated: boolean;
}

const ProgressBarPart = ({ color, percentage, isStriped, isAnimated }: ProgressBarProps) => {
    const classes = [
        progressBar,
        ...isStriped ? [progressBarStriped] : [],
        ...isAnimated ? [progressBarAnimated] : [],
        color
    ]

    return (
        <div style={{ width: `${percentage}%` }} className={classNames(classes)}></div>
    )
}

ProgressBarPart.defaultProps = {
    isStriped: false,
    isAnimated: false
}

export default ProgressBarPart
