"use client";

import Box from '@/app/components/core/layout/box';
import DetailedStepper from '@/app/components/navigation/detailed_stepper';
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

    const steps = [
        {
            label: 'Enter Chat Context',
            component: <StepContext context={context} setContext={setContext} />
        },
        {
            label: 'Select Your Character',
            component: <SelectCharacterView role={Role.User} character={userCharacter} setCharacter={setUserCharacter} />
        },
        {
            label: 'Select Assistant Character',
            component: <SelectCharacterView role={Role.Assistant} character={assistantCharacter} setCharacter={setAssistantCharacter} />
        }
    ];

    const handleStepperComplete = () => {
        setSetupComplete(true);
    }

    const setupDisplay = (
        <DetailedStepper styling={{ m: "medium" }} steps={steps} onComplete={handleStepperComplete} />
    );

    const chatDisplay = userCharacter && assistantCharacter ? (
        <ChatInterfaceView context={context} userCharacter={userCharacter} assistantCharacter={assistantCharacter} />
    ) : "Invalid conversation configuration. Please refresh and try again.";

    return (
        <Box styling={{ width: '100%', height: "100%" }}>
            {setupComplete ? chatDisplay : setupDisplay}
        </Box>
    );
}
