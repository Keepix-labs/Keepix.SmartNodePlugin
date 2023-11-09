import { Routes, Route } from "react-router-dom";
import HomeView from "./views/Home";

export default function App() {
  return (
    <div className="App">
      <Routes>
        <Route path="/" element={<HomeView />} />
      </Routes>
    </div>
  );
}
