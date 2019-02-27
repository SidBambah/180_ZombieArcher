#
# Copyright 2018 Picovoice Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
##### Modified by Sidharth Bambah in 2018 for use in ZombieArcher

import argparse
import os
import platform
import struct
import sys
from datetime import datetime
from threading import Thread

import numpy as np
import pyaudio
import soundfile

sys.path.append(os.path.join(os.path.dirname(__file__), 'porcupine/binding/python'))

from porcupine import Porcupine
global speechValue
global speechCommandNumber
speechValue = "pause"
speechCommandNumber = 0

def recognize():
    class PorcupineProcessor(Thread):
        """
        Class creates an input audio stream from a microphone,
        monitors it, and upon detecting the specified wake word(s) prints the detection time and index of wake word on
        console.
        """

        def __init__(
                self,
                library_path,
                model_file_path,
                keyword_file_paths,
                sensitivities,
                input_device_index=None):

            """
            Constructor.

            :param library_path: Absolute path to Porcupine's dynamic library.
            :param model_file_path: Absolute path to the model parameter file.
            :param keyword_file_paths: List of absolute paths to keyword files.
            :param sensitivities: Sensitivity parameter for each wake word. For more information refer to
            'include/pv_porcupine.h'. It uses the
            same sensitivity value for all keywords.
            :param input_device_index: Optional argument. If provided, audio is recorded from this input device. Otherwise,
            the default audio input device is used.
            """

            super(PorcupineProcessor, self).__init__()

            self._library_path = library_path
            self._model_file_path = model_file_path
            self._keyword_file_paths = keyword_file_paths
            self._sensitivities = sensitivities
            self._input_device_index = input_device_index


        def run(self):
            global speechValue
			global speechCommandNumber
            """
             Creates an input audio stream, initializes wake word detection (Porcupine) object, and monitors the audio
             stream for occurrences of the wake word(s). It prints the time of detection for each occurrence and index of
             wake word.
             """

            num_keywords = len(self._keyword_file_paths)

            keyword_names =\
                [os.path.basename(x).replace('.ppn', '').replace('_tiny', '').split('_')[0] for x in self._keyword_file_paths]

            print('listening for:')
            for keyword_name, sensitivity in zip(keyword_names, sensitivities):
                print('- %s (sensitivity: %f)' % (keyword_name, sensitivity))

            porcupine = None
            pa = None
            audio_stream = None
            try:
                porcupine = Porcupine(
                    library_path=self._library_path,
                    model_file_path=self._model_file_path,
                    keyword_file_paths=self._keyword_file_paths,
                    sensitivities=self._sensitivities)

                pa = pyaudio.PyAudio()
                audio_stream = pa.open(
                    rate=porcupine.sample_rate,
                    channels=1,
                    format=pyaudio.paInt16,
                    input=True,
                    frames_per_buffer=porcupine.frame_length,
                    input_device_index=self._input_device_index)

                while True:
                    pcm = audio_stream.read(porcupine.frame_length, exception_on_overflow = False)
                    pcm = struct.unpack_from("h" * porcupine.frame_length, pcm)

                    result = porcupine.process(pcm)
                    #if num_keywords == 1 and result:
                        #print('[%s] detected keyword' % str(datetime.now()))
                    if num_keywords > 1 and result >= 0:
                        print('[%s] detected %s' % (str(datetime.now()), keyword_names[result]))
                        speechValue = keyword_names[result].split(' ')[0]
						speechCommandNumber = speechCommandNumber + 1

            except KeyboardInterrupt:
                print('stopping ...')
            finally:
                if porcupine is not None:
                    porcupine.delete()

                if audio_stream is not None:
                    audio_stream.close()

                if pa is not None:
                    pa.terminate()

        _AUDIO_DEVICE_INFO_KEYS = ['index', 'name', 'defaultSampleRate', 'maxInputChannels']

        @classmethod
        def show_audio_devices_info(cls):
            """ Provides information regarding different audio devices available. """

            pa = pyaudio.PyAudio()

            for i in range(pa.get_device_count()):
                info = pa.get_device_info_by_index(i)
                print(', '.join("'%s': '%s'" % (k, str(info[k])) for k in cls._AUDIO_DEVICE_INFO_KEYS))

            pa.terminate()


    parser = argparse.ArgumentParser()

    parser.add_argument(
        '--model_file_path',
        help='absolute path to model parameter file',
        type=str,
        default=os.path.join(os.path.dirname(__file__), 'porcupine/lib/common/porcupine_params.pv'))

    parser.add_argument('--sensitivities', help='detection sensitivity [0, 1]', default=0.5)
    parser.add_argument('--input_audio_device_index', help='index of input audio device', type=int, default=None)

    parser.add_argument('--show_audio_devices_info', action='store_true')

    args = parser.parse_args()

    if args.show_audio_devices_info:
        PorcupineProcessor.show_audio_devices_info()
    else:
        keyword_file_paths = [os.path.abspath("keywords/" + x) for x in os.listdir(os.getcwd() + "/keywords")]

        if isinstance(args.sensitivities, float):
            sensitivities = [args.sensitivities] * len(keyword_file_paths)
        else:
            sensitivities = [float(x) for x in args.sensitivities.split(',')]

        PorcupineProcessor(
            library_path=os.path.join(os.path.dirname(__file__), 'porcupine/lib/linux/%s/libpv_porcupine.so' % 'x86_64'),
            model_file_path=args.model_file_path,
            keyword_file_paths=keyword_file_paths,
            sensitivities=sensitivities,
            input_device_index=args.input_audio_device_index).run()
            