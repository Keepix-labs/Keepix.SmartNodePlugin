import { useEffect, useState } from "react";
import Btn from "../components/Btn/Btn";
import Field from "../components/Field/Field";
import "./Home.scss";
import { safeFetch } from "../lib/utils";
import { KEEPIX_API_URL, PLUGIN_API_SUBPATH } from "../constants";
import { useQuery } from "@tanstack/react-query";
import { getPluginStatus, getPluginSyncProgress, getPluginWallet } from "../queries/api";
import Sprites from "../components/Sprites/Sprites";
import BigLoader from "../components/BigLoader/BigLoader";
import BannerAlert from "../components/BannerAlert/BannerAlert";
import BigLogo from "../components/BigLogo/BigLogo";
import { Icon } from "@iconify-icon/react";

export default function HomePage() {
  const [loading, setLoading] = useState(false);
  
  const walletQuery = useQuery({
    queryKey: ["getPluginWallet"],
    queryFn: getPluginWallet
  });

  const statusQuery = useQuery({
    queryKey: ["getPluginStatus"],
    queryFn: async () => {
      if (walletQuery.data === undefined) { // wallet check required before anything
        await walletQuery.refetch();
      }
      return getPluginStatus();
    },
    refetchInterval: 2000
  });

  const syncProgressQuery = useQuery({
    queryKey: ["getPluginSyncProgress"],
    queryFn: getPluginSyncProgress,
    refetchInterval: 5000,
    enabled: statusQuery.data?.NodeState === 'NODE_RUNNING'
  });

  return (
    <div className="AppBase-content">
      {(!statusQuery?.data || loading) && (
        <BigLoader title="" full={true}></BigLoader>
      )}
      {statusQuery?.data && statusQuery.data?.NodeState === 'NO_STATE' && (
        <BannerAlert status="danger">Error with the Plugin "{statusQuery.data?.NodeState}" please Reinstall.</BannerAlert>
      )}
      {statusQuery?.data && statusQuery.data?.NodeState === 'NODE_STOPPED' && (
        <BigLogo full={true}>
          <Btn
            status="warning"
            onClick={async () => {
              setLoading(true);
              await safeFetch(`${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/start`);
              setLoading(false);
            }}
          >Start</Btn>
        </BigLogo>
      )}
      {statusQuery?.data
        && statusQuery.data?.NodeState === 'NODE_RUNNING'
        && walletQuery.data?.Wallet === undefined && (<>
          setup wallet
      </>)}

      {statusQuery?.data
        && !syncProgressQuery?.data
        && statusQuery.data?.NodeState === 'NODE_RUNNING'
        && walletQuery.data?.Wallet !== undefined && (
        <BigLoader title="" label="Retrieving synchronization information" full={true}></BigLoader>
      )}
      {statusQuery?.data
        && syncProgressQuery?.data
        && syncProgressQuery?.data?.IsSynced === false
        && statusQuery.data?.NodeState === 'NODE_RUNNING'
        && walletQuery.data?.Wallet !== undefined && (
        <BigLoader title="Synchonization In Progress" disableLabel={true} full={true}>
          <div className="state-title">
                <strong>{`ExecutionSyncProgress: ${syncProgressQuery?.data.executionSyncProgress}%`}</strong>
                <strong>{`ConsensusSyncProgress: ${syncProgressQuery?.data.consensusSyncProgress}%`}</strong>
                <strong><Icon icon="svg-spinners:3-dots-scale" /></strong>
          </div>
          <Btn
              status="danger"
              onClick={async () => { await safeFetch(`${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/stop`) }}
            >Stop</Btn>
        </BigLoader>
      )}
      {statusQuery?.data && syncProgressQuery?.data
        && syncProgressQuery?.data?.IsSynced === true
        && statusQuery.data?.NodeState === 'NODE_RUNNING'
        && walletQuery.data?.Wallet !== undefined && (<>
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
              onClick={async () => { await safeFetch(`${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/stop`) }}
            >Stop</Btn>
            :
            <Btn
              status="warning"
              onClick={async () => { await safeFetch(`${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/start`) }}
            >Start</Btn>
          }
          <Btn
            status="warning"
            onClick={async () => { await safeFetch(`${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/restart`) }}
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
      </>)}
      <Sprites></Sprites>
    </div>
  );
}
