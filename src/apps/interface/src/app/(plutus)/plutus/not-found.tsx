import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { FileQuestion } from "lucide-react";
import Link from "next/link";

export default function PlutusNotFound() {
  return (
    <div className="flex flex-col items-center justify-center min-h-full gap-4 p-4">
      <Card className="max-w-md w-full">
        <CardContent className="flex flex-col items-center gap-4 p-6 text-center">
          <FileQuestion className="h-10 w-10 text-muted-foreground" />
          <h2 className="text-lg font-semibold">Not found</h2>
          <p className="text-sm text-muted-foreground">
            The Plutus page you&apos;re looking for doesn&apos;t exist.
          </p>
          <Button asChild variant="outline">
            <Link href="/plutus">Back to Markets</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}