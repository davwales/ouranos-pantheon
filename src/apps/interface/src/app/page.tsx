import { AppOverview } from "./_components/app-overview";
import { HealthDashboard } from "./_components/health-dashboard";

export default function Home() {
  return (
    <div className="m-4 space-y-6">
      <AppOverview />
      <HealthDashboard />
    </div>
  );
}
