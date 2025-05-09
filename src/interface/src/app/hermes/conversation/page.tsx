"use client";

import { Step } from '@/app/components/stepper';
import Stepper from '@/app/components/stepper/stepper';
import StepContext from '@/app/hermes/conversation/components/step_context';
import ConversationCharacter from '@/app/hermes/conversation/models/conversation_character';
import ChatInterfaceView from '@/app/hermes/conversation/views/chat_interface_view';
import SelectCharacterView from '@/app/hermes/conversation/views/select_character_view';
import { Role } from '@/gql/graphql';
import { useState } from 'react';

export default function Conversation() {
    const [context, setContext] = useState('');
    const [userCharacter, setUserCharacter] = useState<ConversationCharacter | undefined>();
    const [assistantCharacter, setAssistantCharacter] = useState<ConversationCharacter | undefined>();
    const [setupComplete, setSetupComplete] = useState(false);

    const steps: Step[] = [
        {
            label: 'Context',
            content: <StepContext context={context} setContext={setContext} />
        },
        {
            label: 'Your Character',
            content: <SelectCharacterView role={Role.User} character={userCharacter} setCharacter={setUserCharacter} />
        },
        {
            label: 'Assistant Character',
            content: <SelectCharacterView role={Role.Assistant} character={assistantCharacter} setCharacter={setAssistantCharacter} />
        }
    ];

    const handleStepperComplete = () => {
        setSetupComplete(true);
    }

    const ChatDisplay = () => userCharacter && assistantCharacter ? (
        <ChatInterfaceView context={context} userCharacter={userCharacter} assistantCharacter={assistantCharacter} />
    ) : "Invalid conversation configuration. Please refresh and try again.";

    return (
        <div>
            {setupComplete ? (
                <ChatDisplay />
            ) : (
                <Stepper steps={steps} onComplete={handleStepperComplete} className="m-4" />
            )}
        </div>
    );
}
