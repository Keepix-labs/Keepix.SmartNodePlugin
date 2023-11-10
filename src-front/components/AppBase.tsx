import { ReactNode } from "react";
import "./AppBase.scss";
import { Icon } from "@iconify-icon/react";

type Props = {
  title: string;
  subTitle?: string;
  icon?: string;
  color?: string;
  children: ReactNode;
};

export default function AppsBase({
  title,
  subTitle,
  icon = "ph:house-simple",
  color,
  children,
}: Props) {
  const style = { "--color": color } as React.CSSProperties;

  return (
    <>
      <header className="AppBase-header" style={style}>
        <div className="AppBase-headerIcon icon-app">
          <Icon icon={icon} />
        </div>
        <div className="AppBase-headerContent">
          <h1 className="AppBase-headerTitle">{title}</h1>
          {subTitle && <div className="AppBase-headerSubtitle">{subTitle}</div>}
        </div>
      </header>
      <div className="AppBase-content">{children}</div>
    </>
  );
}
