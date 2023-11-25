import "./Progress.scss";
import { Icon } from '@iconify-icon/react';

type Props = {
  percent?: number;
}

export default function Progress({ percent = 0 }: Props) {
  return (
    <div className={`progress ${percent === 100 ? 'complete' : percent}`}>
      <Icon className="progressIcon" icon={percent === 100 ? "ph:check-circle-duotone" : "svg-spinners:6-dots-scale-middle"} />
      <div className="progressBar" style={{ width: `${percent}%` }}></div>
      <span>{percent}%</span>
    </div>
  )
}