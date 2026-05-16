import { ChatMessageSkeleton } from "@/components/shared/skeletons";

export default function HermesChatLoading() {
  return (
    <div className="m-4 space-y-4">
      <ChatMessageSkeleton pairCount={2} />
    </div>
  );
}