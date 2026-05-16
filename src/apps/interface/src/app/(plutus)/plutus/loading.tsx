import { InfoCardGridSkeleton } from "@/components/shared/skeletons";

export default function PlutusLoading() {
  return (
    <div className="m-4 space-y-4">
      <div className="h-8 w-24 bg-muted animate-pulse rounded" aria-hidden="true" />
      <InfoCardGridSkeleton count={3} hasIcon />
    </div>
  );
}