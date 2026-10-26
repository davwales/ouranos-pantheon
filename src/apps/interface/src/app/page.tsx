import { AppOverview } from "./_components/app-overview";
import { HomeDashboard } from "./_components/dashboard";
import { HealthDashboard } from "./_components/health-dashboard";

export default function Home() {
  return (
    <div className="m-4 space-y-6">
      <AppOverview />
      <HomeDashboard />
      <HealthDashboard />
    </div>
  );
}
