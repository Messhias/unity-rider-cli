package com.jetbrains.rider.plugins.unityclipipeline.settings

import com.intellij.openapi.components.PersistentStateComponent
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.State
import com.intellij.openapi.components.Storage

/**
 * Plugin settings for Unity CLI resolution.
 * Mirrors Core UnityCliSettings: optional CLI path override and default test timeout.
 * Environment variables UNITY_CLI_PATH / UNITY_TEST_TIMEOUT remain supported as overrides at resolve time.
 */
@Service(Service.Level.APP)
@State(name = "UnityCliPipelineSettings", storages = [Storage("unityCliPipeline.xml")])
class UnityCliPipelineSettings : PersistentStateComponent<UnityCliPipelineSettings> {
    var cliPath: String = ""
    var defaultTimeoutSeconds: Int = DEFAULT_TIMEOUT_SECONDS

    override fun getState(): UnityCliPipelineSettings = this

    override fun loadState(state: UnityCliPipelineSettings) {
        cliPath = state.cliPath
        defaultTimeoutSeconds = state.defaultTimeoutSeconds
    }

    companion object {
        const val DEFAULT_TIMEOUT_SECONDS: Int = 600
    }
}
