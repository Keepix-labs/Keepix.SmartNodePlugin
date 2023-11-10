import "./Field.scss";
import { ReactNode } from "react";
import { Icon } from "@iconify-icon/react";
import { Link } from "react-router-dom";

type Status = "info" | "success" | "warning" | "danger" | "gray" | "gray-black";

type PropsBtn = {
  href?: string;
  title?: string;
  icon?: string;
  status?: Status;
  color?: string;
  onClick?: () => void;
  children: ReactNode;
};

export default function Field({
  href,
  icon,
  title,
  children,
  status,
  color,
  onClick,
}: PropsBtn) {
  const Content = (
    <>
      {icon && <Icon icon={icon} />}
      {title && <h4>{title}:</h4>}
      <span>{children}</span>
    </>
  );

  if (onClick) {
    return (
      <button onClick={onClick} className="Field-field" data-status={status} style={{color: color}} disabled>
        {Content}
      </button>
    );
  }

  return (
    <button className="Field-field" data-status={status} style={{color: color}}>
      {Content}
    </button>
  );
}
