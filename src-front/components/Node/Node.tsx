import "./Node.scss";
import { Icon } from "@iconify-icon/react";
import Btn from "../Btn/Btn";
import Field from "../Field/Field";
import { KEEPIX_API_URL, PLUGIN_API_SUBPATH } from "../../constants";
import { safeFetch } from "../../lib/utils";
import Web3 from "web3";

export const Node = ({ node, wallet, status }: any) => {

    // const web3 = new Web3();

    return (<>
        <div className="card card-default">
            <header className="AppBase-header">
                <div className="AppBase-headerIcon icon-app">
                <Icon icon="logos:ethereum-color" />
                </div>
                <div className="AppBase-headerContent">
                <h1 className="AppBase-headerTitle">Your Ethereum Node</h1>
                <div className="AppBase-headerSubtitle">Informations</div>
                </div>
            </header>
            <div className="home-row-full" >
                <Field
                icon="pajamas:status-health"
                status="success"
                title="Status"
                >{status?.NodeState}</Field>
            </div>
            <div className="home-row-full" >
            <Field
                status="success"
                title="Wallet Address"
                icon="ion:wallet"
            >{ wallet }</Field>
            </div>
            <div className="home-row-full" >
                <Btn
                    icon="logos:metamask-icon"
                    status="info"
                    href="http://192.168.1.20:30300"
                    target="_blank"
                    color="white"
                >Your Own RPC: http://192.168.1.20:30300</Btn>
            </div>
            <div className="home-row-2" >
            {
                (status?.NodeState !== 'NODE_STOPPED' && status?.NodeState !== 'NO_STATE') ?
                <Btn
                icon="material-symbols:stop"
                status="danger"
                onClick={async () => { await safeFetch(`${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/stop`) }}
                >Stop</Btn>
                :
                <Btn
                icon="mdi:play"
                status="warning"
                onClick={async () => { await safeFetch(`${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/start`) }}
                >Start</Btn>
            }
            <Btn
                icon="mdi:restart"
                status="warning"
                onClick={async () => { await safeFetch(`${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/restart`) }}
            >Restart</Btn>
            </div>
        </div>
    </>);
}