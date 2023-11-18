import { useEffect, useState } from "react";
import AppsBase from "../components/AppBase";
import Btn from "../components/Btn/Btn";
import Field from "../components/Field/Field";
import "./Home.scss";
import { safeFetch } from "../lib/utils";
import { KEEPIX_API_URL } from "../constants";
import { useQuery } from "@tanstack/react-query";
import { getPluginStatus, getPluginWallet } from "../queries/api";

export default function HomeView() {
  const statusQuery = useQuery({
    queryKey: ["getPluginStatus"],
    queryFn: getPluginStatus,
    refetchInterval: 2000
  });

  const walletQuery = useQuery({
    queryKey: ["getPluginWallet"],
    queryFn: getPluginWallet
  });

  return (
    <AppsBase
      title="Smart Node Plugin"
      subTitle="DashBoard"
      icon="ph:sliders-horizontal"
    >
      <div className="home-row-3" >
        <Field
          icon="pajamas:status-health"
          status="success"
          title="Status"
        >{statusQuery?.data?.NodeState}</Field>
        {
          (statusQuery.data?.NodeState !== 'NODE_STOPPED' && statusQuery.data?.NodeState !== 'NO_STATE') ?
          <Btn
            status="danger"
            onClick={async () => { await safeFetch(`${KEEPIX_API_URL}/stop`) }}
          >Stop</Btn>
          :
          <Btn
            status="warning"
            onClick={async () => { await safeFetch(`${KEEPIX_API_URL}/start`) }}
          >Start</Btn>
        }
        <Btn
          status="warning"
          onClick={async () => { await safeFetch(`${KEEPIX_API_URL}/restart`) }}
        >Restart</Btn>
      </div>
      <div className="home-row-full" >
        <Field
          status="success"
          title="Wallet"
          icon="ion:wallet"
        >{ walletQuery.data?.Wallet ?? 'No Wallet' }</Field>
      </div>
      <div className="home-row-2" >
        <Field
          status="gray-black" color="white"
          title="Rewards"
          icon="formkit:ethereum"
        >0 ETH</Field>
        <Field
          status="gray-black" color="white"
          title="Withdrawable"
          icon="formkit:ethereum"
        >0 ETH</Field>
      </div>
      <div className="home-row-2" >
        <Field
          status="gray-black" color="white"
          title="Stake"
          icon="formkit:ethereum"
        >0 ETH</Field>
        <Field
          status="gray-black" color="white"
          title="Stake RPL"
          icon="material-symbols:rocket"
        >0 ETH</Field>
      </div>
      <div className="home-row-2" >
        <Field icon="uil:chart" status="gray-black" color="white">
          Metrics:
        </Field>
        <Field
          status="gray" color="white"
          title="Block"
          icon="clarity:block-line"
        >1,511,511</Field>
      </div>
      <div className="home-row-full" >
        <Btn
          status="gray"
          icon="ph:link"
          color="white"
        >MiniPool Link</Btn>
      </div>
      <div className="home-row-full" >
        <Btn
          status="gray"
          icon="ph:link"
          color="white"
        >Grafana</Btn>
      </div>
      <div className="home-row-full" >
        <Btn
          status="gray"
          icon="ph:link"
          color="white"
        >More ...</Btn>
      </div>
      <div className="home-row-full" >
        <Field icon="bi:three-dots" status="gray-black" color="white">
            Others:
        </Field>
      </div>
      <div className="home-row-full" >
        <Btn
          status="info"
          href="http://192.168.1.20:30300"
          color="white"
        >Your Own RPC: http://192.168.1.20:30300</Btn>
      </div>
    </AppsBase>
  );
}
