import "./MiniPool.scss";
import { Icon } from "@iconify-icon/react";
import Btn from "../Btn/Btn";
import Field from "../Field/Field";

export const MiniPool = ({ index, total, pool, wallet }: any) => {
    return (<>
        <div className="card card-default">
            <header className="AppBase-header">
                <div className="AppBase-headerIcon icon-app">
                <Icon icon="material-symbols:rocket" />
                </div>
                <div className="AppBase-headerContent">
                <h1 className="AppBase-headerTitle">MiniPool ({index} / {total})</h1>
                <div className="AppBase-headerSubtitle">MiniPool {pool['Address']}</div>
                </div>
            </header>
            <div className="home-row-full" >
            <Field
                status="success"
                title="Minipool Address"
                icon="ion:wallet"
            >{ pool['Address'] }</Field>
            </div>
            <div className="home-row-full" >
            <Field
                status="gray-black" color="white"
                title="Rewards"
                icon="formkit:ethereum"
            >{ pool['Total-EL-rewards'] }</Field>
            </div>
            <div className="home-row-full" >
            <Field
                status="gray-black" color="white"
                title="Portion"
                icon="formkit:ethereum"
            >{ pool['Your-portion'] }</Field>
            </div>
            {/* <div className="home-row-2" >
            <Field icon="uil:chart" status="gray-black" color="white">
                Metrics:
            </Field>
            <Field
                status="gray" color="white"
                title="Block"
                icon="clarity:block-line"
            >1,511,511</Field>
            </div> */}
            <div className="home-row-full" >
            <Btn
                status="gray"
                icon="ph:link"
                color="white"
            >MiniPool Link</Btn>
            </div>
        </div>
    </>);
}