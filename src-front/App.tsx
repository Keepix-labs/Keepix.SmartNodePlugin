import { Routes, Route } from "react-router-dom";
import HomeView from "./views/Home";

export default function App() {
  return (
    <div className="App">
      <Routes>
        <Route path={process.env.PUBLIC_URL + "/"} element={<HomeView />} />
      </Routes>
    </div>
  );
}
