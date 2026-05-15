import { InfoCardGridSkeleton } from "@/components/shared/skeletons";

export default function HermesLoading() {
  return (
    <div className="m-4">
      <InfoCardGridSkeleton count={5} />
    </div>
  );
}