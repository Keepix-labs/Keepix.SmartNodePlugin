import { KEEPIX_API_URL, PLUGIN_API_SUBPATH } from "../constants";

// Plugin
export const getPluginStatus = async () =>
  request<any>({
    url: `${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/status`,
    method: 'GET',
    name: "getPluginStatus",
    parser: (data: any) => { return JSON.parse(data.result); } 
  });

export const getPluginWallet = async () => 
  request<any>({
    url: `${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/wallet-fetch`,
    method: 'GET',
    name: "getPluginWallet",
    parser: (data: any) => { return JSON.parse(data.result); } 
  });

export const getPluginSyncProgress = async () => 
  request<any>({
    url: `${KEEPIX_API_URL}${PLUGIN_API_SUBPATH}/sync-2`,
    method: 'GET',
    name: "getPluginSyncProgress",
    parser: (data: any) => { return JSON.parse(data.result); } 
  });

// Functions
async function request<T>(options: any) {
  if (options.method === undefined) {
    options.method = 'GET';
  }
  const response: Response = await fetch(options.url, {
    method: options.method,
    headers: {
      "Content-Type": "application/json",
    },
    body: options.method === 'POST' && options.body !== undefined ? JSON.stringify(options.body): undefined
  });

  if (!response.ok) {
    throw new Error(`${options.name} call failed.`);
  }

  const data: T = await response.json();

  if (options.parser !== undefined) {
    return options.parser(data);
  }
  return data;
}
