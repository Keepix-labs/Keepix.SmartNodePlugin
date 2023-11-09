// constants.ts
let currentHost = "";
let currentProtocol = "";

// if (process.browser) {
currentHost = window.location.hostname;
currentProtocol = window.location.protocol;
// }

export const KEEPIX_API_URL =
  process.env.REACT_APP_API_URL || `${currentProtocol}//${currentHost}:${currentProtocol === 'http:' ? 2000 : 9000}`;
